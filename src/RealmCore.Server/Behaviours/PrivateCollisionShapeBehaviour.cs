namespace RealmCore.Server.Behaviours;

// TODO:
internal sealed class PrivateCollisionShapeBehaviour
{
    //private readonly List<MarkerElementComponent> _markerElementComponents = new();
    //private readonly List<MarkerElementComponent> _markerElementsToRemove = new();
    //private readonly ReaderWriterLockSlim _markerElementComponentsLock = new();
    //private readonly Task _refreshPrivateCollisionShapeCollidersTask;
    //private readonly ILogger<PrivateCollisionShapeBehaviour> _logger;
    //private readonly IElementCollection _elementCollection;

    //public PrivateCollisionShapeBehaviour(IElementCollection elementCollection, IElementFactory elementFactory, ILogger<PrivateCollisionShapeBehaviour> logger)
    //{
    //    foreach (var player in elementCollection.GetByType<Player>().Cast<RealmPlayer>())
    //    {
    //        var privateMarkerElementComponents = player.GetComponents<PlayerPrivateElementComponent<MarkerElementComponent>>();
    //        foreach (var markerElementComponent in privateMarkerElementComponents)
    //        {
    //            Add(markerElementComponent);
    //        }
    //    }
    //    elementFactory.ElementCreated += HandleElementCreated;
    //    _refreshPrivateCollisionShapeCollidersTask = Task.Run(RefreshPrivateCollisionShapeColliders);
    //    _logger = logger;
    //    _elementCollection = elementCollection;
    //}

    //private void HandleElementCreated(IElementFactory arg1, Element arg2)
    //{
    //    throw new NotImplementedException();
    //}

    //private void HandleEntityCreated(Entity entity)
    //{
    //    entity.Disposed += HandleDisposed;
    //    entity.ComponentAdded += HandleComponentAdded;
    //}

    //private void HandleDisposed(Entity entity)
    //{
    //    entity.Disposed -= HandleDisposed;
    //    entity.ComponentAdded -= HandleComponentAdded;
    //}

    //private void HandleComponentAdded(IComponent component)
    //{
    //    if (component is PlayerPrivateElementComponent<MarkerElementComponent> privateMarkerElementComponent)
    //    {
    //        lock (_markerElementComponents)
    //            Add(privateMarkerElementComponent);
    //    }
    //}

    //private void Add(PlayerPrivateElementComponent<MarkerElementComponent> markerElementComponent)
    //{
    //    _markerElementComponentsLock.EnterWriteLock();
    //    try
    //    {
    //        _markerElementComponents.Add(markerElementComponent.ElementComponent);
    //    }
    //    finally
    //    {
    //        _markerElementComponentsLock.ExitWriteLock();
    //    }
    //}

    //private void HandleMarkerElementComponentDisposed(Component component)
    //{
    //    if (component is PlayerPrivateElementComponent<MarkerElementComponent> privateMarkerElementComponent)
    //    {
    //        _markerElementsToRemove.Add(privateMarkerElementComponent.ElementComponent);
    //    }
    //}

    //private async Task RefreshPrivateCollisionShapeColliders()
    //{
    //    while (true)
    //    {
    //        _markerElementComponentsLock.EnterWriteLock();
    //        try
    //        {
    //            if (_markerElementsToRemove.Count > 0)
    //            {
    //                foreach (var item in _markerElementsToRemove)
    //                {
    //                    _markerElementComponents.Remove(item);
    //                }
    //                _markerElementsToRemove.Clear();
    //            }

    //            // TODO: run outside write lock
    //            foreach (var markerElementComponent in _markerElementComponents)
    //            {
    //                var elements = _elementCollection.GetWithinRange(markerElementComponent.CollisionShape.Position, markerElementComponent.CollisionShape.Radius);
    //                foreach (var element in elements)
    //                    markerElementComponent.CollisionShape.CheckElementWithin(element);
    //            }
    //        }
    //        catch(Exception ex)
    //        {
    //            _logger.LogHandleError(ex);
    //        }
    //        finally
    //        {
    //            _markerElementComponentsLock.ExitWriteLock();
    //        }

    //        await Task.Delay((int)(1000.0f / 60.0f));
    //    }
    //}
}
