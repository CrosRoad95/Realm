using Realm.Domain;
using Realm.Domain.Components.Elements;

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

    public Entity CreatePlayerEntity()
    {
        var entity = new Entity(_serviceProvider, Guid.NewGuid().ToString()[..8], Entity.EntityTag.Player);
        entity.AddComponent(new PlayerElementComponent(_testingServer.AddFakePlayer()));
        return entity;
    }
}
