using RealmCore.Resources.Overlay;

namespace RealmCore.Sample.Components;

internal class DurationBasedHoldInteractionWithRingEffectComponent : DurationBasedHoldInteractionComponent
{
    public override TimeSpan Time => TimeSpan.FromSeconds(5);

    private readonly object _lock = new();
    private readonly IOverlayService _overlayService;
    private string? _ringId = null;
    public DurationBasedHoldInteractionWithRingEffectComponent(IOverlayService overlayService)
    {
        InteractionStarted += HandleInteractionStarted;
        InteractionCompleted += HandleInteractionCompleted;
        _overlayService = overlayService;
    }

    private void HandleInteractionStarted(DurationBasedHoldInteractionComponent durationBasedHoldInteractionComponent, RealmPlayer player, TimeSpan time)
    {
        lock (_lock)
        {
            _ringId = _overlayService.AddRing3dDisplay(Owner, Element.Position, time);
        }
    }

    private void HandleInteractionCompleted(DurationBasedHoldInteractionComponent durationBasedHoldInteractionComponent, RealmPlayer player, bool succeed)
    {
        lock (_lock)
            if (_ringId != null)
            {
                _overlayService.RemoveRing3dDisplay(player, _ringId);
                _ringId = null;
            }
    }
}
