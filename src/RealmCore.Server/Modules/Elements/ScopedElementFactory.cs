﻿namespace RealmCore.Server.Modules.Elements;

internal sealed class InnerScopedElementFactory : ScopedElementFactory
{
    public InnerScopedElementFactory(PlayerContext playerContext, ScopedMapIdGenerator elementIdGenerator) : base(playerContext, elementIdGenerator) { }

    public override IScopedElementFactory CreateScope() => throw new NotSupportedException();
}

internal class ScopedElementFactory : IScopedElementFactory
{
    private readonly RealmPlayer _player;
    private bool _disposed;
    public event Action<ScopedElementFactory>? Disposed;
    public event Action<IElementFactory, Element>? ElementCreated;
    private readonly object _lock = new();
    private readonly List<Element> _createdElements = [];
    private readonly List<Element> _collisionDetection = [];
    private readonly ScopedMapIdGenerator _elementIdGenerator;

    public RealmPlayer Player => _player;
    
    public IEnumerable<Element> CreatedElements
    {
        get
        {
            lock (_lock)
            {
                foreach (var element in _createdElements)
                {
                    yield return element;
                }
            }
        }
    }

    public IEnumerable<Element> CreatedCollisionDetectionElements
    {
        get
        {
            lock (_lock)
            {
                Element[] elements = [.. _collisionDetection];
                foreach (var collisionShape in elements)
                {
                    yield return collisionShape;
                }
            }
        }
    }

    public ScopedElementFactory(PlayerContext playerContext, ScopedMapIdGenerator elementIdGenerator)
    {
        _player = playerContext.Player;
        _player.Destroyed += HandlePlayerDestroyed;
        _elementIdGenerator = elementIdGenerator;
    }

    private void HandleInnerElementCreated(IElementFactory elementFactory, Element createdElement)
    {
        Add(createdElement);
    }

    public virtual IScopedElementFactory CreateScope()
    {
        var innerScope = new ScopedElementFactory(_player.GetRequiredService<PlayerContext>(), _elementIdGenerator);
        innerScope.ElementCreated += HandleInnerElementCreated;

        void handleDisposed(ScopedElementFactory scopedElementFactory)
        {
            innerScope.ElementCreated -= HandleInnerElementCreated;
        }
        innerScope.Disposed += handleDisposed;
        return innerScope;
    }

    private void HandlePlayerDestroyed(Element _)
    {
        _player.Destroyed -= HandlePlayerDestroyed;
        Dispose();
    }

    private void Add(Element element)
    {
        if (element.Id.Value == 0)
            element.Id = (ElementId)_elementIdGenerator.GetId();

        lock (_lock)
        {
            _createdElements.Add(element);
            if (element is Pickup or Marker or CollisionShape)
                _collisionDetection.Add(element);
            element.Destroyed += HandleDestroyed;
        }
    }

    private void HandleDestroyed(Element element)
    {
        lock (_lock)
        {
            element.Destroyed -= HandleDestroyed;
            _createdElements.Remove(element);
            if (element is Pickup or Marker or CollisionShape)
                _collisionDetection.Remove(element);
        }
    }

    public void AssociateWithServer(Element element)
    {
        throw new NotSupportedException();
    }

    public void AssociateWithPlayer(Element element)
    {
        Add(element);
        element.AssociateWith(_player);
        if (element is RealmMarker pickup)
        {
            if (pickup.CollisionShape.Id.Value == 0)
                pickup.CollisionShape.Id = (ElementId)_elementIdGenerator.GetId();
            pickup.CollisionShape.AssociateWith(_player);
        }

        if (element is RealmMarker marker)
        {
            if (marker.CollisionShape.Id.Value == 0)
                marker.CollisionShape.Id = (ElementId)_elementIdGenerator.GetId();
            marker.CollisionShape.AssociateWith(_player);
        }

        RelayCreated(element);
    }

    public RealmCollisionSphere CreateCollisionSphere(Vector3 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionSphere>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var collisionSphere = new RealmCollisionSphere(position, radius)
        {
            Interior = interior ?? _player.Interior,
            Dimension = dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(collisionSphere);
        AssociateWithPlayer(collisionSphere);
        return collisionSphere;
    }

    public RealmMarker CreateMarker(Location location, MarkerType markerType, float size, Color color, Action<RealmMarker>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var marker = new RealmMarker(location.Position, markerType, size)
        {
            Color = color,
            Interior = location.Interior ?? _player.Interior,
            Dimension = location.Dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(marker);
        AssociateWithPlayer(marker);
        return marker;
    }

    public RealmBlip CreateBlip(Location location, BlipIcon blipIcon, Action<RealmBlip>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var blip = new RealmBlip(location.Position, blipIcon)
        {
            Interior = location.Interior ?? _player.Interior,
            Dimension = location.Dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(blip);
        AssociateWithPlayer(blip);
        return blip;
    }

    public RealmCollisionCircle CreateCollisionCircle(Vector2 position, float radius, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCircle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionCuboid CreateCollisionCuboid(Vector3 position, Vector3 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionCuboid>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionPolygon CreateCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices, byte? interior = null, ushort? dimension = null, Action<RealmCollisionPolygon>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionRectangle CreateCollisionRectangle(Vector2 position, Vector2 dimensions, byte? interior = null, ushort? dimension = null, Action<RealmCollisionRectangle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmCollisionTube CreateCollisionTube(Vector3 position, float radius, float height, byte? interior = null, ushort? dimension = null, Action<RealmCollisionTube>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPed CreatePed(Location location, PedModel pedModel, Action<RealmPed>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmPickup CreatePickup(Location location, ushort model, Action<RealmPickup>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var pickup = new RealmPickup(location.Position, model)
        {
            Interior = location.Interior ?? _player.Interior,
            Dimension = location.Dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(pickup);
        AssociateWithPlayer(pickup);
        return pickup;
    }

    public RealmRadarArea CreateRadarArea(Vector2 position, Vector2 size, Color color, byte? interior = null, ushort? dimension = null, Action<RealmRadarArea>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmVehicle CreateVehicle(Location location, VehicleModel model, Action<RealmVehicle>? elementBuilder = null)
    {
        throw new NotImplementedException();
    }

    public RealmWorldObject CreateObject(Location location, ObjectModel model, Action<RealmWorldObject>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var worldObject = new RealmWorldObject(model, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.Interior ?? _player.Interior,
            Dimension = location.Dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(worldObject);
        AssociateWithPlayer(worldObject);
        return worldObject;
    }

    public FocusableRealmWorldObject CreateFocusableObject(Location location, ObjectModel model, Action<RealmWorldObject>? elementBuilder = null)
    {
        ThrowIfDisposed();
        var worldObject = new FocusableRealmWorldObject(model, location.Position)
        {
            Rotation = location.Rotation,
            Interior = location.Interior ?? _player.Interior,
            Dimension = location.Dimension ?? _player.Dimension
        };

        elementBuilder?.Invoke(worldObject);
        AssociateWithPlayer(worldObject);
        return worldObject;
    }

    public void RelayCreated(Element element)
    {
        ElementCreated?.Invoke(this, element);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        List<Element> elementsToRemove;
        lock (_lock)
            elementsToRemove = new(_createdElements);

        foreach (var element in elementsToRemove)
        {
            element.Destroy();
        }
        Disposed?.Invoke(this);
        _disposed = true;
    }

    public Task<RealmVehicle> CreateVehicle(Location location, VehicleModel model, Func<RealmVehicle, Task> elementBuilder)
    {
        throw new NotImplementedException();
    }
}
