namespace RealmCore.BlazorGui.Concepts;

internal class DurationBasedHoldWithRingEffectInteraction : DurationBasedHoldInteraction
{
    public override TimeSpan Time => TimeSpan.FromSeconds(3);

    private readonly IOverlayService _overlayService;
    public DurationBasedHoldWithRingEffectInteraction(IOverlayService overlayService)
    {
        InteractionStarted += HandleInteractionStarted;
        InteractionCompleted += HandleInteractionCompleted;
        _overlayService = overlayService;
    }

    private void HandleInteractionStarted(DurationBasedHoldInteraction durationBasedHoldInteraction, RealmPlayer player, TimeSpan time, CancellationToken cancellationToken)
    {
        if (Owner == null)
            return;

        var ringId = _overlayService.AddRing3dDisplay(Owner, Owner.Position, time);
        cancellationToken.Register(() =>
        {
            _overlayService.RemoveRing3dDisplay(player, ringId);
        });
    }

    private void HandleInteractionCompleted(DurationBasedHoldInteraction durationBasedHoldInteractionComponent, RealmPlayer player, bool succeed)
    {
    }
}
