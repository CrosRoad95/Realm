namespace Realm.Persistance.Services;

public class PeriodicEntitySaveService
{
    private readonly SemaphoreSlim semaphore = new(1);
    private readonly HashSet<ISavable> _playerAccountsToSave = new();
    private readonly HashSet<ISavable> _persistantVehiclesToSave = new();
    private readonly ILogger _logger;
    public PeriodicEntitySaveService(ILogger logger)
    {
        _logger = logger.ForContext<PeriodicEntitySaveService>();
        var _ = Task.Run(PeriodicSaveEntities); // TODO: Implement cancelation
    }

    private async Task PeriodicSaveEntities()
    {
        List<ISavable> workingCopyOfPlayerAccounts = new();
        List<ISavable> workingCopyOfPersistantVehicles = new();
        while(true)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await semaphore.WaitAsync();
            if (_playerAccountsToSave.Any())
            {
                workingCopyOfPlayerAccounts = _playerAccountsToSave.Cast<ISavable>().ToList();
                _playerAccountsToSave.Clear();
            }
            
            if (_persistantVehiclesToSave.Any())
            {
                workingCopyOfPersistantVehicles = _persistantVehiclesToSave.Cast<ISavable>().ToList();
                _persistantVehiclesToSave.Clear();
            }

            semaphore.Release();

            if(workingCopyOfPlayerAccounts.Any())
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
    
    public void VehicleCreated(IPersistantVehicle persistantVehicle)
    {
        persistantVehicle.NotifyNotSavedState += PersistantVehicle_DirtyNotify;
        persistantVehicle.Disposed += PersistantVehicle_Disposed;
    }

    private async void PlayerAccount_DirtyNotify(ISavable playerAccount)
    {
        await ScheduleAccountToSave(playerAccount);
    }

    private async void PlayerAccount_Disposed(PlayerAccount playerAccount)
    {
        playerAccount.NotifyNotSavedState -= PlayerAccount_DirtyNotify;
        playerAccount.Disposed -= PlayerAccount_Disposed;
        await ScheduleAccountToSave(playerAccount);
    }
    
    private async void PersistantVehicle_DirtyNotify(ISavable persistantVehicle)
    {
        await ScheduleVehicleToSave(persistantVehicle);
    }

    private async void PersistantVehicle_Disposed(IPersistantVehicle playerAccount)
    {
        playerAccount.NotifyNotSavedState -= PersistantVehicle_DirtyNotify;
        playerAccount.Disposed -= PersistantVehicle_Disposed;
        await ScheduleAccountToSave(playerAccount);
    }

    private async Task ScheduleAccountToSave(ISavable playerAccount)
    {
        await semaphore.WaitAsync();
        _playerAccountsToSave.Add(playerAccount);
        semaphore.Release();
        _logger.Verbose("Scheduled account: {playerAccount} to save.", playerAccount);
    }

    private async Task ScheduleVehicleToSave(ISavable persistantVehicle)
    {
        await semaphore.WaitAsync();
        _persistantVehiclesToSave.Add(persistantVehicle);
        semaphore.Release();
        _logger.Verbose("Scheduled vehicle: {peristantVehicle} to save.", persistantVehicle);
    }
}
