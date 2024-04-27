namespace RealmCore.Server.Modules.World.Triggers;

internal sealed class PlayerScopedCollisionShapeBehaviour : IDisposable
{
    private RealmPlayer _player;
    private readonly IScopedElementFactory _scopedElementFactory;
    private readonly ILogger<PlayerScopedCollisionShapeBehaviour> _logger;

    public PlayerScopedCollisionShapeBehaviour(PlayerContext playerContext, IScopedElementFactory scopedElementFactory, ILogger<PlayerScopedCollisionShapeBehaviour> logger)
    {
        _player = playerContext.Player;

        _player.PositionChanged += HandlePositionChanged;
        _scopedElementFactory = scopedElementFactory;
        _logger = logger;
    }

    private void HandlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        foreach (var collisionDetection in _scopedElementFactory.CreatedCollisionDetectionElements)
        {
            switch (collisionDetection)
            {
                case CollisionShape collisionShape:
                    collisionShape.CheckElementWithin(_player);
                    break;
                case RealmMarker marker:
                    marker.CollisionShape.CheckElementWithin(_player);
                    break;
                case RealmPickup pickup:
                    pickup.CollisionShape.CheckElementWithin(_player);
                    break;
            }
        }
    }

    public void Dispose()
    {
        _player.PositionChanged -= HandlePositionChanged;
    }
}

internal sealed class ScopedCollisionShapeBehaviour : PlayerLifecycle
{
    public ScopedCollisionShapeBehaviour(PlayersEventManager playersEventManager) : base(playersEventManager)
    {
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.GetRequiredService<PlayerScopedCollisionShapeBehaviour>();
    }
}
