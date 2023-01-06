using Realm.Persistance.Data;

namespace Realm.Server.Services;

internal class SignInService : ISignInService
{
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

    private readonly HashSet<Guid> _usedAccountsIds = new();
    public SignInService()
    {

    }

    public async Task<bool> SignIn(Entity entity, User user)
    {
        if (entity.Tag != Entity.PlayerTag || !entity.HasComponent<PlayerElementComponent>())
            throw new NotSupportedException("Entity is not a player entity.");

        await _lock.WaitAsync();
        try
        {
            if (!_usedAccountsIds.Add(user.Id))
                return false;

            await entity.AddComponentAsync(new AccountComponent(user));
            if (user.Inventory != null)
                await entity.AddComponentAsync(new InventoryComponent(user.Inventory));
            else
                await entity.AddComponentAsync(new InventoryComponent(20));

            if (user.DailyVisits != null)
                await entity.AddComponentAsync(new DailyVisitsCounterComponent(user.DailyVisits));
            else
                await entity.AddComponentAsync(new DailyVisitsCounterComponent());
            
            if (user.Statistics != null)
                await entity.AddComponentAsync(new StatisticsCounterComponent(user.Statistics));
            else
                await entity.AddComponentAsync(new StatisticsCounterComponent());
            
            if (user.Achievements != null)
                await entity.AddComponentAsync(new AchievementsComponent(user.Achievements));
            else
                await entity.AddComponentAsync(new AchievementsComponent());

            entity.AddComponent(new LicensesComponent(user.Licenses, user.Id));
            entity.AddComponent(new PlayTimeComponent());
            entity.AddComponent(new MoneyComponent(user.Money));
            entity.AddComponent(new AFKComponent()).StateChanged += (entity, wentAfk, time) =>
            {
                System.Console.WriteLine("Afk state: {0}", wentAfk);
            };
            entity.Destroyed += HandleDestroyed;
        }
        catch (Exception)
        {
            // TODO: add Entity component add scope thing
            _usedAccountsIds.Remove(user.Id);
            entity.TryDestroyComponent<AccountComponent>();
            entity.TryDestroyComponent<InventoryComponent>();
            entity.TryDestroyComponent<LicensesComponent>();
            entity.TryDestroyComponent<PlayTimeComponent>();
            entity.TryDestroyComponent<MoneyComponent>();
            entity.TryDestroyComponent<DailyVisitsCounterComponent>();
            entity.TryDestroyComponent<StatisticsCounterComponent>();
        }
        finally
        {
            _lock.Release();
        }
        return true;
    }

    private async void HandleDestroyed(Entity entity)
    {
        await _lock.WaitAsync();
        try
        {
            var accountComponent = entity.GetRequiredComponent<AccountComponent>();
            _usedAccountsIds.Remove(accountComponent.Id);
        }
        catch(Exception ex)
        {
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}
