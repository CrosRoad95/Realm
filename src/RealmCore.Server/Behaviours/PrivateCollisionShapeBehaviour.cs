using RealmCore.Server.Components.Elements;
using static Grpc.Core.Metadata;

namespace RealmCore.Server.Behaviours;

internal sealed class PrivateCollisionShapeBehaviour
{
    private readonly List<MarkerElementComponent> _markerElementComponents = new();
    private readonly object _markerElementComponentsLock = new();
    private readonly Task _refreshPrivateCollisionShapeCollidersTask;

    public PrivateCollisionShapeBehaviour(MtaServer server, IElementCollection elementCollection, IECS ecs)
    {
        lock (_markerElementComponentsLock)
        {
            foreach (var playerEntity in ecs.Entities.Where(x => x.Tag == EntityTag.Player))
            {
                var privaterMarkerElementComponents = playerEntity.GetComponents<PlayerPrivateElementComponent<MarkerElementComponent>>();
                foreach (var markerElementComponent in privaterMarkerElementComponents)
                {
                    Add(markerElementComponent);
                }
            }
        }

        ecs.EntityCreated += HandleEntityCreated;
        _refreshPrivateCollisionShapeCollidersTask = Task.Run(RefreshPrivateCollisionShapeColliders);
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
        _markerElementComponents.Add(markerElementComponent.ElementComponent);
        markerElementComponent.Disposed += HandleMarkerElementComponentDisposed;
    }

    private void HandleMarkerElementComponentDisposed(Component component)
    {
        if (component is PlayerPrivateElementComponent<MarkerElementComponent> privateMarkerElementComponent)
            lock (_markerElementComponentsLock)
            {
                _markerElementComponents.Remove(privateMarkerElementComponent.ElementComponent);
            }
    }

    private async Task RefreshPrivateCollisionShapeColliders()
    {
        while (true)
        {
            lock (_markerElementComponentsLock)
            {
                foreach (var markerElementComponent in _markerElementComponents)
                {
                    markerElementComponent.RefreshColliders();
                }
            }
            await Task.Delay(250);
        }
    }
}
