using Microsoft.AspNetCore.Identity;
using RealmCore.Persistence.Data;
using RealmCore.Server.Components.TagComponents;
using System.Security.Claims;

namespace RealmCore.Tests.Helpers;

internal class EntityHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TestingServer _testingServer;

    public EntityHelper(TestingServer testingServer)
    {
        _testingServer = testingServer;
        _serviceProvider = _testingServer.GetRequiredService<IServiceProvider>();
    }

    public Entity CreatePlayerEntity()
    {
        var entity = new Entity();
        var player = _testingServer.AddFakePlayer();
        player.Name = "CrosRoad95";
        player.TriggerResourceStarted(420);
        entity.AddComponent<Transform>();
        entity.AddComponent<PlayerTagComponent>();
        entity.AddComponent(new PlayerElementComponent(player, new Vector2(1920, 1080), new System.Globalization.CultureInfo("pl-PL"), _testingServer.GetRequiredService<IEntityEngine>(), _serviceProvider.GetRequiredService<IDateTimeProvider>()));

        return entity;
    }

    public async Task<UserComponent> LogInEntity(Entity entity, string[]? roles = null)
    {
        var claims = new List<Claim>
        {
            new("test", "true"),
        };
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<RoleData>>();
        foreach (var roleName in new string[] { "Admin" })
        {
            await roleManager.CreateAsync(new RoleData
            {
                Name = roleName
            });
        }

        var userManager = _serviceProvider.GetRequiredService<UserManager<UserData>>();

        var user = new UserData
        {
            UserName = $"userName{Guid.NewGuid()}",
            Upgrades = new List<UserUpgradeData>(),
        };
        await userManager.CreateAsync(user);
        await userManager.AddClaimsAsync(user, claims);
        var userComponent = await entity.AddComponentAsync(new UserComponent(user, _serviceProvider.GetRequiredService<SignInManager<UserData>>(), _serviceProvider.GetRequiredService<UserManager<UserData>>()));
        if(roles != null)
        {
            foreach (var item in roles)
            {
                await userComponent.AddRole(item);
            }
        }
        return userComponent;
    }

    public Entity CreateObjectEntity()
    {
        var entity = new Entity();
        entity.AddComponent<Transform>();
        entity.AddComponent<WorldObjectTagComponent>();
        entity.AddComponent(new WorldObjectComponent(new SlipeServer.Server.Elements.WorldObject(SlipeServer.Server.Enums.ObjectModel.Vegtree3, Vector3.Zero)));
        return entity;
    }
}
