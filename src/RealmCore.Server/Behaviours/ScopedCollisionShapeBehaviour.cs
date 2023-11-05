namespace RealmCore.Server.Behaviours;

internal sealed class ScopedCollisionShapeBehaviour
{
    private readonly ILogger<ScopedCollisionShapeBehaviour> _logger;

    public ScopedCollisionShapeBehaviour(MtaServer mtaServer, ILogger<ScopedCollisionShapeBehaviour> logger)
    {
        mtaServer.PlayerJoined += HandlePlayerJoined;
        _logger = logger;
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (RealmPlayer)plr;
        var scopedElementFactory = player.GetRequiredService<IScopedElementFactory>();


        void handleElementCreated(Element element)
        {
            if (element is not ICollisionDetection collisionShape)
                return;

            void handleElementEntered(Element enteredElement)
            {
                if (enteredElement != player)
                    return;
                try
                {
                    if (collisionShape.InternalCollisionDetection.CheckRules(enteredElement))
                    {
                        collisionShape.InternalCollisionDetection.RelayEntered(enteredElement);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogHandleError(ex);
                }
            }

            void handleElementLeft(Element leftElement)
            {
                if (leftElement != player)
                    return;

                collisionShape.InternalCollisionDetection.RelayLeft(leftElement);
            }

            collisionShape.ElementEntered += handleElementEntered;
            collisionShape.ElementLeft += handleElementLeft;
        }

        void handlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
        {
            foreach (var collisionShape in scopedElementFactory.CollisionShapes)
            {
                collisionShape.CheckElementWithin(player);
            }
        }
        void handleDisconnected(Player sender, PlayerQuitEventArgs e)
        {
            player.Disconnected -= handleDisconnected;
            player.PositionChanged -= handlePositionChanged;
        }
        scopedElementFactory.ElementCreated += handleElementCreated;
        player.Disconnected += handleDisconnected;
        player.PositionChanged += handlePositionChanged;
    }
}
