using RealmCore.Server.Components;
using RealmCore.Server.Components.Elements.CollisionShapes;
using RealmCore.Server.Enums;
using RealmCore.Server.Rules;
using SlipeServer.Server.Enums;

namespace RealmCore.Console.Logic;

internal class WorldLogic
{
    private struct DiscoveryInfo
    {
        public BlipIcon blipIcon;
        public Vector3 position;

        public DiscoveryInfo(BlipIcon blipIcon, Vector3 position)
        {
            this.blipIcon = blipIcon;
            this.position = position;
        }
    }

    private readonly Dictionary<int, DiscoveryInfo> _discoveryInfos = new Dictionary<int, DiscoveryInfo>
    {
        [1] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(327.11523f, -65.00488f, 1.5703526f)),
        [2] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(307.69336f, -71.15039f, 1.4296875f)),
        [3] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(283.7129f, -71.95996f, 1.4339179f)),
    };

    private readonly IEntityFactory _entityFactory;

    public WorldLogic(IRPGServer rpgServer, IEntityFactory entityFactory, IECS ecs)
    {
        _entityFactory = entityFactory;
        rpgServer.ServerStarted += HandleServerStarted;
        ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag != EntityTag.Player)
            return;

        entity.ComponentAdded += HandleComponentAdded;
        entity.Disposed += HandleDestroyed;
    }

    private void HandleDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is DiscoveriesComponent discoveriesComponent)
        {
            foreach (var discovery in discoveriesComponent.Discoveries)
            {
                HandlePlayerDiscover(component.Entity, discovery);
            }
        }
    }

    private void HandleServerStarted()
    {
        _entityFactory.CreateRadarArea(new Vector2(0, 0), new Vector2(50, 50), System.Drawing.Color.Pink);

        _entityFactory.CreateObject((ObjectModel)1338, new Vector3(0, 0, 10), Vector3.Zero);
        var veh = _entityFactory.CreateVehicle(404, new Vector3(0, 0, 4), Vector3.Zero);
        var ped = _entityFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Cj, new Vector3(5, 0, 4));
        veh.GetRequiredComponent<VehicleElementComponent>().AddPassenger(0, ped);

        foreach (var item in _discoveryInfos)
        {
            var entity = _entityFactory.CreateCollisionSphere(item.Value.position, 8);
            AttachDiscovery(entity, item.Key, HandlePlayerDiscover);
        }
    }

    private void HandlePlayerDiscover(Entity entity, int discoverId, bool newlyDiscovered = false)
    {
        if (newlyDiscovered)
        {
            entity.GetRequiredComponent<PlayerElementComponent>().AddNotification($"Pomyślnie odkryłes miejscowke: {discoverId}");
        }

        if (_discoveryInfos.TryGetValue(discoverId, out var value))
        {
            _entityFactory.CreateBlipFor(entity, value.blipIcon, value.position);
        }
    }

    private void AttachDiscovery(Entity entity, int discoveryName, Action<Entity, int, bool> callback)
    {
        var collisionSphereElementComponent = entity.GetRequiredComponent<CollisionSphereElementComponent>();
        collisionSphereElementComponent.AddRule<MustBePlayerRule>();
        collisionSphereElementComponent.EntityEntered = enteredEntity =>
        {
            if (enteredEntity.GetRequiredComponent<DiscoveriesComponent>().TryDiscover(discoveryName))
            {
                callback(enteredEntity, discoveryName, true);
            }
        };
    }
}
