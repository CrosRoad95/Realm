using Realm.Domain.Components.Elements;
using Realm.Domain.Enums;
using System.Numerics;

namespace Realm.Tests.Helpers;

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

        entity.AddComponent(new PlayerElementComponent(player, new System.Numerics.Vector2(1920, 1080), new System.Globalization.CultureInfo("pl-PL")));

        Task.Run(async () =>
        {
            await Task.Delay(30000);
            for (ushort i = 0; i < ushort.MaxValue; i++)
                player.TriggerResourceStarted(i);

        });

        return entity;
    }

    public Entity CreateObjectEntity()
    {
        var entity = new Entity(_serviceProvider, Guid.NewGuid().ToString()[..8], EntityTag.Player);
        entity.AddComponent(new WorldObjectComponent(new SlipeServer.Server.Elements.WorldObject(SlipeServer.Server.Enums.ObjectModel.Vegtree3, Vector3.Zero)));
        return entity;
    }
}
