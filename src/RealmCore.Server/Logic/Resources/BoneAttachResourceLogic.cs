namespace RealmCore.Server.Logic.Resources;

internal sealed class BoneAttachResourceLogic : PlayerLogic
{
    private readonly BoneAttachService _boneAttachService;
    private readonly MtaServer _mtaServer;

    public BoneAttachResourceLogic(BoneAttachService boneAttachService, MtaServer mtaServer) : base(mtaServer)
    {
        _boneAttachService = boneAttachService;
        _mtaServer = mtaServer;
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

    private void HandleWorldObjectDetached(RealmPlayer player, AttachedBoneWorldObject attachedBoneWorldObject)
    {
        _boneAttachService.Detach(attachedBoneWorldObject.WorldObject);
    }

    private void HandleWorldObjectAttached(RealmPlayer player, AttachedBoneWorldObject attachedBoneWorldObject)
    {
        _boneAttachService.Attach(attachedBoneWorldObject.WorldObject, player, attachedBoneWorldObject.BoneId, attachedBoneWorldObject.PositionOffset, attachedBoneWorldObject.RotationOffset);
        attachedBoneWorldObject.WorldObject.AreCollisionsEnabled = false; // TODO: remember previous state
    }
}
