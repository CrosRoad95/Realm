using RealmCore.Resources.Overlay;
using RealmCore.Server.Components.Elements.CollisionShapes;
using RealmCore.Server.Components.Players;
using SlipeServer.Server.Enums;

namespace RealmCore.Sample.Logic;

internal class WorldLogic : ComponentLogic<DiscoveriesComponent>
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

    private readonly Dictionary<int, DiscoveryInfo> _discoveryInfos = new()
    {
        [1] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(327.11523f, -65.00488f, 1.5703526f)),
        [2] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(307.69336f, -71.15039f, 1.4296875f)),
        [3] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(283.7129f, -71.95996f, 1.4339179f)),
    };

    private readonly IEntityFactory _entityFactory;
    private readonly IOverlayService _overlayService;

    public WorldLogic(IRealmServer realmServer, IEntityFactory entityFactory, IEntityEngine entityEngine, IOverlayService overlayService) : base(entityEngine)
    {
        _entityFactory = entityFactory;
        _overlayService = overlayService;
        realmServer.ServerStarted += HandleServerStarted;
    }

    protected override void ComponentAdded(DiscoveriesComponent discoveriesComponent)
    {
        foreach (var discovery in discoveriesComponent.Discoveries)
        {
            HandlePlayerDiscover(discoveriesComponent.Entity, discovery);
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
            _overlayService.AddNotification(entity, $"Pomyślnie odkryłes miejscowke: {discoverId}");
        }

        if (_discoveryInfos.TryGetValue(discoverId, out var value))
        {
            using var scopedEntityFactory = _entityFactory.CreateScopedEntityFactory(entity);
            scopedEntityFactory.CreateBlip(value.blipIcon, value.position);
        }
    }

    private void AttachDiscovery(Entity entity, int discoveryName, Action<Entity, int, bool> callback)
    {
        var collisionSphereElementComponent = entity.GetRequiredComponent<CollisionSphereElementComponent>();
        collisionSphereElementComponent.AddRule<MustBePlayerRule>();
        collisionSphereElementComponent.EntityEntered = (enteredColshapeEntity, enteredEntity) =>
        {
            if (enteredEntity.GetRequiredComponent<DiscoveriesComponent>().TryDiscover(discoveryName))
            {
                callback(enteredEntity, discoveryName, true);
            }
        };
    }
}
