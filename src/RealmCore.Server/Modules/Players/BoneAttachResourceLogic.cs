namespace RealmCore.Server.Modules.Players;

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
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.WorldObjectAttached -= HandleWorldObjectAttached;
    }

    private void HandleWorldObjectAttached(RealmPlayer player, AttachedBoneWorldObject attachedBoneWorldObject)
    {
        _boneAttachService.Attach(attachedBoneWorldObject.WorldObject, player, attachedBoneWorldObject.BoneId, attachedBoneWorldObject.PositionOffset, attachedBoneWorldObject.RotationOffset);
        attachedBoneWorldObject.WorldObject.AreCollisionsEnabled = false; // TODO: remember previous state
    }
}
