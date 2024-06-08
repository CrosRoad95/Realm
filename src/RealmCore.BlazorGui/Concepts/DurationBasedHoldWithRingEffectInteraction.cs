namespace RealmCore.BlazorGui.Concepts;

internal class DurationBasedHoldWithRingEffectInteraction : DurationBasedHoldInteraction
{
    public override TimeSpan Time => TimeSpan.FromSeconds(5);

    private readonly object _lock = new();
    private readonly IOverlayService _overlayService;
    private string? _ringId = null;
    public DurationBasedHoldWithRingEffectInteraction(IOverlayService overlayService)
    {
        InteractionStarted += HandleInteractionStarted;
        InteractionCompleted += HandleInteractionCompleted;
        _overlayService = overlayService;
    }

    private void HandleInteractionStarted(DurationBasedHoldInteraction durationBasedHoldInteraction, RealmPlayer player, TimeSpan time)
    {
        lock (_lock)
        {
            if (Owner == null)
                return;
            _ringId = _overlayService.AddRing3dDisplay(Owner, Owner.Position, time);
        }
    }

    private void HandleInteractionCompleted(DurationBasedHoldInteraction durationBasedHoldInteractionComponent, RealmPlayer player, bool succeed)
    {
        lock (_lock)
            if (_ringId != null)
            {
                _overlayService.RemoveRing3dDisplay(player, _ringId);
                _ringId = null;
            }
    }
}
