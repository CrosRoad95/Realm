using Realm.Persistance.Scripting.Classes;
using System.Threading;

namespace Realm.Persistance.Services;

public class PeriodicEntitySaveService
{
    private readonly SemaphoreSlim semaphore = new(1);
    private readonly HashSet<PlayerAccount> _playerAccountsToSave = new();
    private readonly ILogger _logger;
    public PeriodicEntitySaveService(ILogger logger)
    {
        _logger = logger.ForContext<PeriodicEntitySaveService>();
        var _ = Task.Run(PeriodicSaveEntities); // TODO: Implement cancelation
    }

    private async Task PeriodicSaveEntities()
    {
        List<PlayerAccount> workingCopyOfPlayerAccounts = new();
        while(true)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            await semaphore.WaitAsync();
            if (_playerAccountsToSave.Any())
            {
                workingCopyOfPlayerAccounts = _playerAccountsToSave.ToList();
                _playerAccountsToSave.Clear();
                semaphore.Release();
            }
            else
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

        }
    }

    public void AccountCreated(PlayerAccount playerAccount)
    {
        playerAccount.DirtyNotify += PlayerAccount_DirtyNotify;
        playerAccount.Disposed += PlayerAccount_Disposed;
    }

    private async void PlayerAccount_Disposed(PlayerAccount playerAccount)
    {
        playerAccount.DirtyNotify -= PlayerAccount_DirtyNotify;
        playerAccount.Disposed -= PlayerAccount_Disposed;
        await ScheduleAccountToSave(playerAccount);
    }

    private async Task ScheduleAccountToSave(PlayerAccount playerAccount)
    {
        await semaphore.WaitAsync();
        _playerAccountsToSave.Add(playerAccount);
        semaphore.Release();
    }

    private async void PlayerAccount_DirtyNotify(PlayerAccount playerAccount)
    {
        await ScheduleAccountToSave(playerAccount);
        _logger.Verbose("Scheduled account: {playerAccount} to save.", playerAccount);
    }
}
