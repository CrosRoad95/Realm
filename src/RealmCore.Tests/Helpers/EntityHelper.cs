using RealmCore.Server.Components.Elements;
using System.Numerics;

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
        var entity = new Entity(_serviceProvider, Guid.NewGuid().ToString()[..8], EntityTag.Player);
        var player = _testingServer.AddFakePlayer();
        player.TriggerResourceStarted(420);
        entity.AddComponent(new PlayerElementComponent(player, new Vector2(1920, 1080), new System.Globalization.CultureInfo("pl-PL")));

        return entity;
    }

    public Entity CreateObjectEntity()
    {
        var entity = new Entity(_serviceProvider, Guid.NewGuid().ToString()[..8], EntityTag.Player);
        entity.AddComponent(new WorldObjectComponent(new SlipeServer.Server.Elements.WorldObject(SlipeServer.Server.Enums.ObjectModel.Vegtree3, Vector3.Zero)));
        return entity;
    }
}
