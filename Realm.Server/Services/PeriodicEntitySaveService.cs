namespace Realm.Server.Services;

public class PeriodicEntitySaveService
{
    private readonly SemaphoreSlim semaphore = new(1);
    private readonly HashSet<PlayerAccount> _playerAccountsToSave = new();
    private readonly HashSet<RPGVehicle> _persistantVehiclesToSave = new();
    private readonly ILogger _logger;
    public PeriodicEntitySaveService(ILogger logger)
    {
        _logger = logger.ForContext<PeriodicEntitySaveService>();
        var _ = Task.Run(PeriodicSaveEntities); // TODO: Implement cancelation
    }

    private async Task PeriodicSaveEntities()
    {
        List<PlayerAccount> workingCopyOfPlayerAccounts = new();
        List<RPGVehicle> workingCopyOfPersistantVehicles = new();
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await semaphore.WaitAsync();
            if (_playerAccountsToSave.Any())
            {
                workingCopyOfPlayerAccounts = _playerAccountsToSave.Cast<PlayerAccount>().ToList();
                _playerAccountsToSave.Clear();
            }

            if (_persistantVehiclesToSave.Any())
            {
                workingCopyOfPersistantVehicles = _persistantVehiclesToSave.Cast<RPGVehicle>().ToList();
                _persistantVehiclesToSave.Clear();
            }

            semaphore.Release();

            if (workingCopyOfPlayerAccounts.Any())
            {
                // Save account in try catch, report errors, maybe reschedule save
                foreach (var playerAccount in workingCopyOfPlayerAccounts)
                {
                    await playerAccount.Save();
                }
                _logger.Verbose("Saved {count} accounts.", workingCopyOfPlayerAccounts.Count);
                workingCopyOfPlayerAccounts.Clear();
            }

            if (workingCopyOfPersistantVehicles.Any())
            {
                foreach (var persistantVehicle in workingCopyOfPersistantVehicles)
                {
                    await persistantVehicle.Save();
                }
                _logger.Verbose("Saved {count} vehicles.", workingCopyOfPersistantVehicles.Count);
                workingCopyOfPersistantVehicles.Clear();
            }
        }
    }

    public void AccountCreated(PlayerAccount playerAccount)
    {
        playerAccount.NotifyNotSavedState += PlayerAccount_DirtyNotify;
        playerAccount.Disposed += PlayerAccount_Disposed;
    }

    public void VehicleCreated(RPGVehicle persistantVehicle)
    {
        persistantVehicle.NotifyNotSavedState += PersistantVehicle_DirtyNotify;
        persistantVehicle.Disposed += PersistantVehicle_Disposed;
    }

    private async void PlayerAccount_DirtyNotify(PlayerAccount playerAccount)
    {
        await ScheduleAccountToSave(playerAccount);
    }

    private async void PlayerAccount_Disposed(PlayerAccount playerAccount)
    {
        playerAccount.NotifyNotSavedState -= PlayerAccount_DirtyNotify;
        playerAccount.Disposed -= PlayerAccount_Disposed;
        await ScheduleAccountToSave(playerAccount);
    }

    private async void PersistantVehicle_DirtyNotify(RPGVehicle persistantVehicle)
    {
        await ScheduleVehicleToSave(persistantVehicle);
    }

    private async void PersistantVehicle_Disposed(RPGVehicle rpgVehicle)
    {
        rpgVehicle.NotifyNotSavedState -= PersistantVehicle_DirtyNotify;
        rpgVehicle.Disposed -= PersistantVehicle_Disposed;
        await ScheduleVehicleToSave(rpgVehicle);
    }

    private async Task ScheduleAccountToSave(PlayerAccount playerAccount)
    {
        await semaphore.WaitAsync();
        _playerAccountsToSave.Add(playerAccount);
        semaphore.Release();
        _logger.Verbose("Scheduled account: {playerAccount} to save.", playerAccount);
    }

    private async Task ScheduleVehicleToSave(RPGVehicle persistantVehicle)
    {
        await semaphore.WaitAsync();
        _persistantVehiclesToSave.Add(persistantVehicle);
        semaphore.Release();
        _logger.Verbose("Scheduled vehicle: {peristantVehicle} to save.", persistantVehicle);
    }
}
