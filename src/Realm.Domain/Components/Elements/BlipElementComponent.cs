namespace Realm.Domain.Components.Elements;

public class BlipElementComponent : ElementComponent
{
    protected readonly Blip _blip;

    internal override Element Element => _blip;

    public BlipIcon BlipIcon
    {
        get
        {
            ThrowIfDisposed();
            return _blip.Icon;
        }
        set
        {
            ThrowIfDisposed();
            _blip.Icon = value;
        }
    }

    public ushort VisibleDistance
    {
        get
        {
            ThrowIfDisposed();
            return _blip.VisibleDistance;
        }
        set
        {
            ThrowIfDisposed();
            _blip.VisibleDistance = value;
        }
    }

    public byte Size
    {
        get
        {
            ThrowIfDisposed();
            return _blip.Size;
        }
        set
        {
            ThrowIfDisposed();
            _blip.Size = value;
        }
    }
    
    public Color Color
    {
        get
        {
            ThrowIfDisposed();
            return _blip.Color;
        }
        set
        {
            ThrowIfDisposed();
            _blip.Color = value;
        }
    }

    internal BlipElementComponent(Blip blip)
    {
        _blip = blip;
    }
}
