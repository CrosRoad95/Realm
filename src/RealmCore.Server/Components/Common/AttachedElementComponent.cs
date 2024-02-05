namespace RealmCore.Server.Components.Common;

public class AttachedElementComponent : ComponentLifecycle
{
    private readonly BoneId _boneId;
    private readonly Vector3? _positionOffset;
    private readonly Vector3? _rotationOffset;
    private readonly object _lock = new();
    public Element? AttachedElement { get; private set; }

    public BoneId BoneId => _boneId;

    public Vector3? PositionOffset => _positionOffset;

    public Vector3? RotationOffset => _rotationOffset;

    public AttachedElementComponent(Element element, BoneId boneId, Vector3? positionOffset = null, Vector3? rotationOffset = null)
    {
        AttachedElement = element;
        AttachedElement.Destroyed += HandleDestroyed;
        _boneId = boneId;
        _positionOffset = positionOffset;
        _rotationOffset = rotationOffset;
    }

    private void HandleDestroyed(Element element)
    {
        if(AttachedElement != null)
        {
            AttachedElement.Destroyed -= HandleDestroyed;
            AttachedElement = null;
            ((IComponents)Element).Components.TryDestroyComponent(this);
        }
    }

    public override void Detach()
    {
        if (AttachedElement != null)
        {
            base.Detach();
            TryDetach(out var _);
        }
    }

    public bool TryDetach(out Element? element)
    {
        lock (_lock)
        {
            if(AttachedElement != null)
            {
                base.Detach();
                AttachedElement.Destroyed -= HandleDestroyed;
                element = AttachedElement;
                AttachedElement = null;
                ((IComponents)Element).Components.TryDestroyComponent(this);
                return true;
            }
            element = null;
            return false;
        }
    }
}
