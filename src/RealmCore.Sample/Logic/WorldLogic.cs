namespace RealmCore.Sample.Logic;

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

    private readonly Dictionary<int, DiscoveryInfo> _discoveryInfoDictionary = new()
    {
        [1] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(327.11523f, -65.00488f, 1.5703526f)),
        [2] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(307.69336f, -71.15039f, 1.4296875f)),
        [3] = new DiscoveryInfo(BlipIcon.Pizza, new Vector3(283.7129f, -71.95996f, 1.4339179f)),
    };

    private readonly IElementFactory _elementFactory;
    private readonly IOverlayService _overlayService;

    public WorldLogic(IElementFactory elementFactory, MtaServer server)
    {
        _elementFactory = elementFactory;
        HandleServerStarted();
        server.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (RealmPlayer)plr;
        foreach (var discovery in player.Discoveries)
        {
            HandlePlayerDiscover(player, discovery);
        }
    }

    private void HandleServerStarted()
    {
        // TODO:
        //_elementFactory.CreateRadarArea(new Vector2(0, 0), new Vector2(50, 50), System.Drawing.Color.Pink);

        //_elementFactory.CreateObject((ObjectModel)1338, new Vector3(0, 0, 10), Vector3.Zero);
        //var veh = _elementFactory.CreateVehicle(404, new Vector3(0, 0, 4), Vector3.Zero);
        //var ped = _elementFactory.CreatePed(SlipeServer.Server.Elements.Enums.PedModel.Cj, new Vector3(5, 0, 4));
        //veh.GetRequiredComponent<VehicleElementComponent>().AddPassenger(0, (Ped)ped);

        //foreach (var item in _discoveryInfoDictionary)
        //{
        //    var collisionSphere = _elementFactory.CreateCollisionSphere(item.Value.position, 8);
        //    AttachDiscovery(collisionSphere, item.Key, HandlePlayerDiscover);
        //}
    }

    private void HandlePlayerDiscover(RealmPlayer player, int discoverId, bool newlyDiscovered = false)
    {
        if (newlyDiscovered)
        {
            _overlayService.AddNotification(player, $"Pomyślnie odkryłes miejscowke: {discoverId}");
        }

        if (_discoveryInfoDictionary.TryGetValue(discoverId, out var value))
        {
            // TODO:
            //using var scopedelementFactory = _elementFactory.CreateScopedelementFactory(player);
            //scopedelementFactory.CreateBlip(value.blipIcon, value.position);
        }
    }

    private void AttachDiscovery(RealmCollisionSphere collisionSphere, int discoveryId, Action<RealmPlayer, int, bool> callback)
    {
        collisionSphere.ElementEntered += (enteredColShape, args) =>
        {
            if(args.Element is RealmPlayer player)
            {
                if (player.Discoveries.TryDiscover(discoveryId))
                {
                    callback(player, discoveryId, true);
                }
            }
        };
    }
}
