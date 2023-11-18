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

        void handleElementCreated(Element element)
        {
            if (element is not ICollisionDetection collisionDetection)
                return;

            void handleElementEntered(Element enteredElement)
            {
                if (enteredElement != player)
                    return;
                try
                {
                    if (collisionDetection.InternalCollisionDetection.CheckRules(enteredElement))
                    {
                        collisionDetection.InternalCollisionDetection.RelayEntered(enteredElement);
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

                collisionDetection.InternalCollisionDetection.RelayLeft(leftElement);
            }

            collisionDetection.ElementEntered += handleElementEntered;
            collisionDetection.ElementLeft += handleElementLeft;
        }

        void handlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
        {
            foreach (var collisionDetection in player.ElementFactory.CreatedCollisionDetectionElements.ToList())
            {
                collisionDetection.CheckElementWithin(player);
            }
        }

        void handleDisconnected(Player sender, PlayerQuitEventArgs e)
        {
            player.Disconnected -= handleDisconnected;
            player.PositionChanged -= handlePositionChanged;
        }
        player.ElementFactory.ElementCreated += handleElementCreated;
        player.Disconnected += handleDisconnected;
        player.PositionChanged += handlePositionChanged;
    }
}
