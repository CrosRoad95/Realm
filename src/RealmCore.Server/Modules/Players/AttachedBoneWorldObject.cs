namespace RealmCore.Server.Modules.Players;

public class AttachedBoneWorldObject
{
    private readonly RealmWorldObject _worldObject;
    private readonly BoneId _boneId;
    private readonly Vector3? _positionOffset;
    private readonly Vector3? _rotationOffset;

    public RealmWorldObject WorldObject => _worldObject;
    public BoneId BoneId => _boneId;

    public Vector3? PositionOffset => _positionOffset;

    public Vector3? RotationOffset => _rotationOffset;

    public AttachedBoneWorldObject(RealmWorldObject worldObject, BoneId boneId, Vector3? positionOffset = null, Vector3? rotationOffset = null)
    {
        _worldObject = worldObject;
        _boneId = boneId;
        _positionOffset = positionOffset;
        _rotationOffset = rotationOffset;
    }
}
