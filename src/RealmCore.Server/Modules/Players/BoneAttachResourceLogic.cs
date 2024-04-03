namespace RealmCore.Server.Modules.Players;

internal sealed class BoneAttachResourceLogic : PlayerLifecycle
{
    private readonly BoneAttachService _boneAttachService;

    public BoneAttachResourceLogic(BoneAttachService boneAttachService, PlayersEventManager playersEventManager) : base(playersEventManager)
    {
        _boneAttachService = boneAttachService;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.WorldObjectAttached += HandleWorldObjectAttached;
        player.WorldObjectDetached += HandleWorldObjectDetached;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.WorldObjectAttached -= HandleWorldObjectAttached;
        player.WorldObjectDetached -= HandleWorldObjectDetached;
    }

    private void HandleWorldObjectAttached(RealmPlayer player, AttachedBoneWorldObject attachedBoneWorldObject)
    {
        _boneAttachService.Attach(attachedBoneWorldObject.WorldObject, player, attachedBoneWorldObject.BoneId, attachedBoneWorldObject.PositionOffset, attachedBoneWorldObject.RotationOffset);
        attachedBoneWorldObject.WorldObject.AreCollisionsEnabled = false; // TODO: remember previous state
    }

    private void HandleWorldObjectDetached(RealmPlayer player, AttachedBoneWorldObject attachedBoneWorldObject)
    {
        if (_boneAttachService.IsAttached(attachedBoneWorldObject.WorldObject))
        {
            _boneAttachService.Detach(attachedBoneWorldObject.WorldObject);
            attachedBoneWorldObject.WorldObject.AreCollisionsEnabled = true;
        }
    }
}
