namespace RealmCore.Server.Components.Common;

[ComponentUsage(true)]
public class AttachedElementComponent : ComponentLifecycle
{
    private readonly BoneId _boneId;
    private readonly Vector3? _positionOffset;
    private readonly Vector3? _rotationOffset;
    private readonly object _lock = new();
    public Element? AttachedElement { get; set; }

    public BoneId BoneId => _boneId;

    public Vector3? PositionOffset => _positionOffset;

    public Vector3? RotationOffset => _rotationOffset;

    public AttachedElementComponent(Element element, BoneId boneId, Vector3? positionOffset = null, Vector3? rotationOffset = null)
    {
        AttachedElement = element;
        _boneId = boneId;
        _positionOffset = positionOffset;
        _rotationOffset = rotationOffset;
    }

    public bool TryDetach(out Element? element)
    {
        lock (_lock)
        {
            if(AttachedElement != null)
            {
                element = AttachedElement;
                AttachedElement = null;
                return true;
            }
            ((IComponents)Element).Components.TryDestroyComponent(this);
            element = null;
            return false;
        }
    }
}
