namespace RealmCore.Server.Components.Elements;

public class RadarAreaElementComponent : ElementComponent
{
    protected readonly RadarArea _radarArea;

    internal override Element Element => _radarArea;

    public Vector2 Size
    {
        get
        {
            ThrowIfDisposed();
            return _radarArea.Size;
        }
        set
        {
            ThrowIfDisposed();
            _radarArea.Size = value;
        }
    }

    public Color Color
    {
        get
        {
            ThrowIfDisposed();
            return _radarArea.Color;
        }
        set
        {
            ThrowIfDisposed();
            _radarArea.Color = value;
        }
    }

    public bool isFlashing
    {
        get
        {
            ThrowIfDisposed();
            return _radarArea.IsFlashing;
        }
        set
        {
            ThrowIfDisposed();
            _radarArea.IsFlashing = value;
        }
    }

    public Vector2 Position2
    {
        get
        {
            ThrowIfDisposed();
            return _radarArea.Position2;
        }
        set
        {
            ThrowIfDisposed();
            _radarArea.Position2 = value;
        }
    }

    public bool IsInside(Entity entity)
    {
        ThrowIfDisposed();
        return _radarArea.IsInside(new Vector2(entity.Transform.Position.X, entity.Transform.Position.Y));
    }

    public bool IsInside(Vector2 vector2)
    {
        ThrowIfDisposed();
        return _radarArea.IsInside(vector2);
    }

    internal RadarAreaElementComponent(RadarArea radarArea)
    {
        _radarArea = radarArea;
    }
}
