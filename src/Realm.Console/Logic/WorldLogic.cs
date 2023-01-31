using Realm.Domain.Components.CollisionShapes;
using Realm.Domain.Components.Object;
using Realm.Domain.Interfaces;
using Realm.Domain.Rules;

namespace Realm.Console.Logic;

internal class WorldLogic
{
    private readonly IEntityFactory _entityFactory;

    public WorldLogic(IRPGServer rpgServer, IEntityFactory entityFactory)
    {
        rpgServer.ServerStarted += HandleServerStarted;
        _entityFactory = entityFactory;
    }

    private void HandleServerStarted()
    {
        var ent1 = _entityFactory.CreateCollisionSphere(new Vector3(327.11523f, -65.00488f, 1.5703526f), 8);
        var ent2 = _entityFactory.CreateCollisionSphere(new Vector3(307.69336f, -71.15039f, 1.4296875f), 8);
        var ent3 = _entityFactory.CreateCollisionSphere(new Vector3(283.7129f, -71.95996f, 1.4339179f), 8);
        AttachDiscovery(ent1, "loc-1", HandlePlayerDiscover);
        AttachDiscovery(ent2, "loc-2", HandlePlayerDiscover);
        AttachDiscovery(ent3, "loc-3", HandlePlayerDiscover);
    }

    private void HandlePlayerDiscover(Entity entity, string discoverId)
    {
        entity.GetRequiredComponent<PlayerElementComponent>().AddNotification($"Pomyślnie odkryłes miejscowke: {discoverId}");
    }

    private void AttachDiscovery(Entity entity, string discoveryName, Action<Entity, string> callback)
    {
        var collisionSphereElementComponent = entity.GetRequiredComponent<CollisionSphereElementComponent>();
        collisionSphereElementComponent.AddRule(new MustBePlayerOnFootOnlyRule());
        collisionSphereElementComponent.EntityEntered = enteredEntity =>
        {
            if (enteredEntity.Tag == Entity.EntityTag.Player && enteredEntity.GetRequiredComponent<DiscoveriesComponent>().TryDiscover(discoveryName))
            {
                callback(enteredEntity, discoveryName);
            }
        };
    }
}
