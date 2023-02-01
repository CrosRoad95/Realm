using Realm.Domain.Inventory;
using Realm.Domain.Registries;

namespace Realm.Server.Services;

internal class SignInService : ISignInService
{
    private readonly SemaphoreSlim _lock = new(1);

    private readonly HashSet<Guid> _usedAccountsIds = new();
    private readonly ItemsRegistry _itemsRegistry;
    private readonly ILogger _logger;

    public SignInService(ItemsRegistry itemsRegistry, ILogger logger)
    {
        _itemsRegistry = itemsRegistry;
        _logger = logger;
    }

    public async Task<bool> SignIn(Entity entity, User user)
    {
        if (entity.Tag != Entity.EntityTag.Player || !entity.HasComponent<PlayerElementComponent>())
            throw new NotSupportedException("Entity is not a player entity.");

        await _lock.WaitAsync();
        try
        {
            if (!_usedAccountsIds.Add(user.Id))
                return false;

            await entity.AddComponentAsync(new AccountComponent(user));
            if (user.Inventories != null && user.Inventories.Any())
            {
                foreach (var inventory in user.Inventories)
                {
                    var items = inventory.InventoryItems
                        .Select(x =>
                            new Item(_itemsRegistry.Get(x.ItemId), x.Number, JsonConvert.DeserializeObject<Dictionary<string, object>>(x.MetaData))
                        )
                        .ToList();
                    await entity.AddComponentAsync(new InventoryComponent(inventory.Size, inventory.Id, items));
                }
            }
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
            
            if (user.JobUpgrades != null)
                await entity.AddComponentAsync(new JobUpgradesComponent(user.JobUpgrades));
            else
                await entity.AddComponentAsync(new JobUpgradesComponent());
            
            if (user.JobStatistics != null)
                await entity.AddComponentAsync(new JobStatisticsComponent(user.JobStatistics));
            else
                await entity.AddComponentAsync(new JobStatisticsComponent());
            
            if (user.Discoveries != null)
                await entity.AddComponentAsync(new DiscoveriesComponent(user.Discoveries));
            else
                await entity.AddComponentAsync(new DiscoveriesComponent());

            foreach (var groupMemberData in user.GroupMembers)
                await entity.AddComponentAsync(new GroupMemberComponent(groupMemberData));
            
            foreach (var fractionMemberData in user.FractionMembers)
                await entity.AddComponentAsync(new FractionMemberComponent(fractionMemberData));

            entity.AddComponent(new LicensesComponent(user.Licenses));
            entity.AddComponent(new PlayTimeComponent());
            entity.AddComponent(new LevelComponent(user.Level, user.Experience));
            entity.AddComponent(new MoneyComponent(user.Money));
            entity.AddComponent(new AFKComponent());
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
            entity.TryDestroyComponent<LevelComponent>();
            entity.TryDestroyComponent<JobUpgradesComponent>();
            entity.TryDestroyComponent<JobStatisticsComponent>();
            entity.TryDestroyComponent<DailyVisitsCounterComponent>();
            entity.TryDestroyComponent<StatisticsCounterComponent>();
            entity.TryDestroyComponent<DiscoveriesComponent>();
            throw;
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
