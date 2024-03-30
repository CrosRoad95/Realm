namespace RealmCore.Server.Modules.World.Triggers;

internal sealed class PlayerScopedCollisionShapeBehaviour : IDisposable
{
    private RealmPlayer _player;
    private readonly ILogger<PlayerScopedCollisionShapeBehaviour> _logger;

    public PlayerScopedCollisionShapeBehaviour(PlayerContext playerContext, IScopedElementFactory scopedElementFactory, ILogger<PlayerScopedCollisionShapeBehaviour> logger)
    {
        _player = playerContext.Player;

        _player.PositionChanged += HandlePositionChanged;
        scopedElementFactory.ElementCreated += HandleElementCreated;
        _logger = logger;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is not ICollisionDetection collisionDetection)
            return;

        void handleElementEntered(Element enteredElement)
        {
            if (enteredElement != _player)
                return;
            try
            {
                collisionDetection.InternalCollisionDetection.RelayEntered(enteredElement);
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }

        void handleElementLeft(Element leftElement)
        {
            if (leftElement != _player)
                return;

            collisionDetection.InternalCollisionDetection.RelayLeft(leftElement);
        }

        collisionDetection.ElementEntered += handleElementEntered;
        collisionDetection.ElementLeft += handleElementLeft;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        foreach (var collisionDetection in _player.ElementFactory.CreatedCollisionDetectionElements.ToList())
        {
            collisionDetection.CheckElementWithin(_player);
        }
    }

    public void Dispose()
    {
        _player.PositionChanged -= HandlePositionChanged;
    }
}

internal sealed class ScopedCollisionShapeBehaviour : PlayerLifecycle
{
    public ScopedCollisionShapeBehaviour(MtaServer mtaServer) : base(mtaServer)
    {
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.GetRequiredService<PlayerScopedCollisionShapeBehaviour>();
    }
}
