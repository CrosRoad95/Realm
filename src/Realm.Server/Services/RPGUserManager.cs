﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Realm.Domain.Inventory;
using Realm.Domain.Options;
using Realm.Domain.Registries;
using Realm.Persistance.Data;

namespace Realm.Server.Services;

internal class RPGUserManager : IRPGUserManager
{
    private readonly SemaphoreSlim _lock = new(1);

    private readonly HashSet<int> _usedAccountsIds = new();
    private readonly ItemsRegistry _itemsRegistry;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RPGUserManager> _logger;
    private readonly IOptions<GameplayOptions> _gameplayOptions;

    public RPGUserManager(ItemsRegistry itemsRegistry, UserManager<User> userManager, ILogger<RPGUserManager> logger, IOptions<GameplayOptions> gameplayOptions)
    {
        _itemsRegistry = itemsRegistry;
        _userManager = userManager;
        _logger = logger;
        _gameplayOptions = gameplayOptions;
    }

    public async Task<User?> SignUp(string username, string password)
    {
        var identityResult = await _userManager.CreateAsync(new User
        {
            UserName = username,
        }, password);
        if (identityResult.Succeeded)
        {
            _logger.LogInformation("Created an account {userName}", username);
            return await _userManager.FindByNameAsync(username) ?? throw new Exception("Unable to find just created account");
        }
        else
        {
            _logger.LogWarning("Failed to create an account {userName} because: {identityResultErrors}", username, identityResult.Errors.Select(x => x.Description));
        }
        
        return null;
    }

    public async Task<bool> SignIn(Entity entity, User user)
    {
        if (entity == null)
            throw new NullReferenceException(nameof(entity));

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
                            new Item(_itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Dictionary<string, object>>(x.MetaData))
                        )
                        .ToList();
                    entity.AddComponent(new InventoryComponent(inventory.Size, inventory.Id, items));
                }
            }
            else
                entity.AddComponent(new InventoryComponent(_gameplayOptions.Value.DefaultInventorySize));

            if (user.DailyVisits != null)
                entity.AddComponent(new DailyVisitsCounterComponent(user.DailyVisits));
            else
                entity.AddComponent<DailyVisitsCounterComponent>();
            
            if (user.Statistics != null)
                entity.AddComponent(new StatisticsCounterComponent(user.Statistics));
            else
                entity.AddComponent<StatisticsCounterComponent>();
            
            if (user.Achievements != null)
                entity.AddComponent(new AchievementsComponent(user.Achievements));
            else
                entity.AddComponent<AchievementsComponent>();
            
            if (user.JobUpgrades != null)
                entity.AddComponent(new JobUpgradesComponent(user.JobUpgrades));
            else
                entity.AddComponent<JobUpgradesComponent>();
            
            if (user.JobStatistics != null)
                entity.AddComponent(new JobStatisticsComponent(user.JobStatistics));
            else
                entity.AddComponent<JobStatisticsComponent>();
            
            if (user.Discoveries != null)
                entity.AddComponent(new DiscoveriesComponent(user.Discoveries));
            else
                entity.AddComponent<DiscoveriesComponent>();
            
            if (user.DiscordIntegration != null)
                entity.AddComponent(new DiscordIntegrationComponent(user.DiscordIntegration.DiscordUserId));

            foreach (var groupMemberData in user.GroupMembers)
                entity.AddComponent(new GroupMemberComponent(groupMemberData));
            
            foreach (var fractionMemberData in user.FractionMembers)
                entity.AddComponent(new FractionMemberComponent(fractionMemberData));

            entity.AddComponent(new LicensesComponent(user.Licenses));
            entity.AddComponent(new PlayTimeComponent(user.PlayTime));
            entity.AddComponent(new LevelComponent(user.Level, user.Experience));
            entity.AddComponent(new MoneyComponent(user.Money));
            entity.AddComponent<AFKComponent>();
        }
        catch (Exception ex)
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
            entity.TryDestroyComponent<DiscordIntegrationComponent>();
            _logger.LogError(ex, "Failed to sign in user of id {userId}", user.Id);
            throw;
        }
        finally
        {
            _lock.Release();
        }
        entity.Destroyed += HandleDestroyed;
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
            _logger.LogError(ex, "Failed to destroy player entity {entityName}", entity.Name);
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}