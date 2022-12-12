namespace Realm.Server.Services;

internal sealed class PeriodicEntitySaveService : IPeriodicEntitySaveService
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
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await Flush();
        }
    }

    public async Task Flush()
    {
        List<PlayerAccount> workingCopyOfPlayerAccounts = new();
        List<RPGVehicle> workingCopyOfPersistantVehicles = new();
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

    public void AccountCreated(PlayerAccount playerAccount)
    {
        playerAccount.NotifyNotSavedState += HandleDirtyNotify;
        playerAccount.Disposed += HandleDisposed;
    }

    public void VehicleCreated(RPGVehicle persistantVehicle)
    {
        persistantVehicle.NotifyNotSavedState += HandleDirtyNotify;
        persistantVehicle.Disposed += HandleDisposed;
    }

    private async void HandleDirtyNotify(PlayerAccount playerAccount)
    {
        await ScheduleAccountToSave(playerAccount);
    }

    private async void HandleDisposed(PlayerAccount playerAccount)
    {
        playerAccount.NotifyNotSavedState -= HandleDirtyNotify;
        playerAccount.Disposed -= HandleDisposed;
        await ScheduleAccountToSave(playerAccount);
    }

    private async void HandleDirtyNotify(RPGVehicle persistantVehicle)
    {
        await ScheduleVehicleToSave(persistantVehicle);
    }

    private async void HandleDisposed(RPGVehicle rpgVehicle)
    {
        rpgVehicle.NotifyNotSavedState -= HandleDirtyNotify;
        rpgVehicle.Disposed -= HandleDisposed;
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
