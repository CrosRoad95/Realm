namespace RealmCore.Server.Concepts.Objectives;

public class MarkerEnterObjective : Objective
{
    private readonly Vector3 _position;

    private PlayerPrivateElementComponent<MarkerElementComponent> _markerElementComponent = default!;
    private PlayerPrivateElementComponent<CollisionSphereElementComponent> _collisionSphereElementComponent = default!;
    private Entity _playerEntity = default!;
    private System.Timers.Timer _checkEnteredTimer = default!;

    public override Vector3 Position => _position;

    public MarkerEnterObjective(Vector3 position)
    {
        _position = position;
    }

    protected override void Load(IEntityFactory entityFactory, Entity playerEntity)
    {
        _playerEntity = playerEntity;
        using var scopedEntityFactory = entityFactory.CreateScopedEntityFactory(playerEntity);
        scopedEntityFactory.CreateMarker(MarkerType.Arrow, _position, Color.White);
        _markerElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<MarkerElementComponent>>();
        scopedEntityFactory.CreateCollisionSphere(_position, 2);
        _collisionSphereElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<CollisionSphereElementComponent>>();
        _collisionSphereElementComponent.ElementComponent.EntityEntered = EntityEntered;
        _checkEnteredTimer = new System.Timers.Timer(TimeSpan.FromSeconds(0.25f));
        _checkEnteredTimer.Elapsed += HandleElapsed;
        _checkEnteredTimer.Start();
    }

    private void HandleElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            _collisionSphereElementComponent.ElementComponent.CheckCollisionWith(_playerEntity);
        }
        catch (ObjectDisposedException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check collision with player entity.");
        }
    }

    private void EntityEntered(Entity colshapeEntity, Entity entity)
    {
        ThrowIfDisposed();

        if (entity == Entity)
            Complete(this);
    }

    public override void Dispose()
    {
        if (_checkEnteredTimer != null)
        {
            _checkEnteredTimer.Stop();
            _checkEnteredTimer.Dispose();
        }
        if (_collisionSphereElementComponent != null)
        {
            _playerEntity.TryDestroyComponent(_collisionSphereElementComponent);
        }
        _playerEntity.TryDestroyComponent(_markerElementComponent);
        base.Dispose();
    }
}
