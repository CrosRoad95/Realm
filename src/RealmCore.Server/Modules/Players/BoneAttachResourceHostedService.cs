namespace RealmCore.Server.Modules.Players;

internal sealed class BoneAttachResourceHostedService : PlayerLifecycle, IHostedService
{
    private readonly BoneAttachService? _boneAttachService;

    public BoneAttachResourceHostedService(PlayersEventManager playersEventManager, BoneAttachService? boneAttachService = null) : base(playersEventManager)
    {
        _boneAttachService = boneAttachService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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
        if (_boneAttachService == null)
            throw new NotSupportedException();

        _boneAttachService.Attach(attachedBoneWorldObject.WorldObject, player, attachedBoneWorldObject.BoneId, attachedBoneWorldObject.PositionOffset, attachedBoneWorldObject.RotationOffset);
        attachedBoneWorldObject.WorldObject.AreCollisionsEnabled = false; // TODO: remember previous state
    }

    private void HandleWorldObjectDetached(RealmPlayer player, AttachedBoneWorldObject attachedBoneWorldObject)
    {
        if (_boneAttachService == null)
            throw new NotSupportedException();

        if (_boneAttachService.IsAttached(attachedBoneWorldObject.WorldObject))
        {
            _boneAttachService.Detach(attachedBoneWorldObject.WorldObject);
            attachedBoneWorldObject.WorldObject.AreCollisionsEnabled = true;
        }
    }
}
