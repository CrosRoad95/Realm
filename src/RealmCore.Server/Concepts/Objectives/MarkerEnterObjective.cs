﻿namespace RealmCore.Server.Concepts.Objectives;

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
        _markerElementComponent = entityFactory.CreateMarkerFor(playerEntity, _position, MarkerType.Arrow, Color.White);
        _collisionSphereElementComponent = entityFactory.CreateCollisionSphereFor(playerEntity, _position, 2);
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
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check collision with player entity.");
        }
    }

    private void EntityEntered(Entity entity)
    {
        ThrowIfDisposed();

        if (entity == Entity)
            Complete(this);
    }

    public override void Dispose()
    {
        if (_checkEnteredTimer != null)
            _checkEnteredTimer.Dispose();
        if (_collisionSphereElementComponent != null)
        {
            _collisionSphereElementComponent.ElementComponent.EntityEntered = null;
            _playerEntity.TryDestroyComponent(_collisionSphereElementComponent);
        }
        _playerEntity.TryDestroyComponent(_markerElementComponent);
        base.Dispose();
    }
}