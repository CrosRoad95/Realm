using Microsoft.AspNetCore.Identity;
using RealmCore.Persistence.Data;
using RealmCore.Server.Components.TagComponents;
using SlipeServer.Server.Clients;
using System.Security.Claims;

namespace RealmCore.Tests.Helpers;

internal class EntityHelper
{
    private readonly TestingServer _testingServer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityEngine _entityEngine;

    public EntityHelper(TestingServer testingServer)
    {
        _testingServer = testingServer;
        _serviceProvider = _testingServer.GetRequiredService<IServiceProvider>();
        _entityEngine = _testingServer.GetRequiredService<IEntityEngine>();
    }

    public Entity CreatePlayerEntity(bool withSerialAndIp = true)
    {
        var entity = _entityEngine.CreateEntity();
        var player = _testingServer.AddFakePlayer();

        if (withSerialAndIp)
        {
            player.Client = new FakeClient(player)
            {
                Serial = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                IPAddress = System.Net.IPAddress.Parse("127.0.0.1")
            };
        }

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
        var signInManager = _serviceProvider.GetRequiredService<SignInManager<UserData>>();

        var user = new UserData
        {
            UserName = $"userName{Guid.NewGuid()}",
            Upgrades = new List<UserUpgradeData>(),
        };
        await userManager.CreateAsync(user);
        await userManager.AddClaimsAsync(user, claims);

        var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);

        if (roles != null)
            foreach (var role in roles)
            {
                if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

        var userComponent = entity.AddComponent(new UserComponent(user, claimsPrincipal));
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
    public Entity CreateVehicleEntity()
    {
        var entity = new Entity();
        entity.AddComponent<Transform>();
        entity.AddComponent<WorldObjectTagComponent>();
        entity.AddComponent(new WorldObjectComponent(new SlipeServer.Server.Elements.WorldObject(SlipeServer.Server.Enums.ObjectModel.Vegtree3, Vector3.Zero)));
        return entity;
    }
}
