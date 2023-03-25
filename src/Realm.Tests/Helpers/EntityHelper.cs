using Realm.Domain.Components.Elements;
using Realm.Domain.Enums;

namespace Realm.Tests.Helpers;

public class EntityHelper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TestingServer _testingServer;

    public EntityHelper(TestingServer testingServer)
    {
        _testingServer = testingServer;
        _serviceProvider = _testingServer.GetRequiredService<IServiceProvider>();
    }

    public AsyncEntity CreatePlayerEntity()
    {
        var entity = new AsyncEntity(_serviceProvider, Guid.NewGuid().ToString()[..8], EntityTag.Player);
        entity.AddComponent(new PlayerElementComponent(_testingServer.AddFakePlayer(), new System.Numerics.Vector2(1920, 1080), new System.Globalization.CultureInfo("pl-PL")));
        return entity;
    }
}
