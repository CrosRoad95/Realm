namespace RealmCore.Server.Behaviours;

internal sealed class PrivateCollisionShapeBehaviour
{
    private readonly List<MarkerElementComponent> _markerElementComponents = new();
    private readonly List<MarkerElementComponent> _markerElementsToRemove = new();
    private readonly ReaderWriterLockSlim _markerElementComponentsLock = new();
    private readonly Task _refreshPrivateCollisionShapeCollidersTask;
    private readonly ILogger<PrivateCollisionShapeBehaviour> _logger;

    public PrivateCollisionShapeBehaviour(IEntityEngine ecs, ILogger<PrivateCollisionShapeBehaviour> logger)
    {
        foreach (var playerEntity in ecs.PlayerEntities)
        {
            var privateMarkerElementComponents = playerEntity.GetComponents<PlayerPrivateElementComponent<MarkerElementComponent>>();
            foreach (var markerElementComponent in privateMarkerElementComponents)
            {
                Add(markerElementComponent);
            }
        }
        ecs.EntityCreated += HandleEntityCreated;
        _refreshPrivateCollisionShapeCollidersTask = Task.Run(RefreshPrivateCollisionShapeColliders);
        _logger = logger;
    }

    private void HandleEntityCreated(Entity entity)
    {
        entity.Disposed += HandleDisposed;
        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleDisposed(Entity entity)
    {
        entity.Disposed -= HandleDisposed;
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is PlayerPrivateElementComponent<MarkerElementComponent> privateMarkerElementComponent)
        {
            lock (_markerElementComponents)
                Add(privateMarkerElementComponent);
        }
    }

    private void Add(PlayerPrivateElementComponent<MarkerElementComponent> markerElementComponent)
    {
        _markerElementComponentsLock.EnterWriteLock();
        try
        {
            _markerElementComponents.Add(markerElementComponent.ElementComponent);
            markerElementComponent.Disposed += HandleMarkerElementComponentDisposed;
        }
        finally
        {
            _markerElementComponentsLock.ExitWriteLock();
        }
    }

    private void HandleMarkerElementComponentDisposed(Component component)
    {
        if (component is PlayerPrivateElementComponent<MarkerElementComponent> privateMarkerElementComponent)
        {
            _markerElementsToRemove.Add(privateMarkerElementComponent.ElementComponent);
        }
    }

    private async Task RefreshPrivateCollisionShapeColliders()
    {
        while (true)
        {
            _markerElementComponentsLock.EnterWriteLock();
            try
            {
                // TODO: run outside write lock
                foreach (var markerElementComponent in _markerElementComponents)
                {
                    markerElementComponent.RefreshColliders();
                }

                if(_markerElementsToRemove.Count > 0)
                {
                    foreach (var item in _markerElementsToRemove)
                    {
                        _markerElementComponents.Remove(item);
                    }
                    _markerElementsToRemove.Clear();
                }
            }
            catch(Exception ex)
            {
                _logger.LogHandleError(ex);
            }
            finally
            {
                _markerElementComponentsLock.ExitWriteLock();
            }

            await Task.Delay((int)(1000.0f / 60.0f));
        }
    }
}
