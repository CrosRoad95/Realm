﻿using Realm.Resources.Overlay;
using Realm.Server.Extensions;

namespace Realm.Console.Components;

internal class DurationBasedHoldInteractionWithRingEffectComponent : DurationBasedHoldInteractionComponent
{
    [Inject]
    private OverlayService OverlayService { get; set; } = default!;
    [Inject]
    private ILogger<DurationBasedHoldInteractionWithRingEffectComponent> Logger { get; set; } = default!;

    private readonly object _lock = new();

    private string? _ringId = null;
    public DurationBasedHoldInteractionWithRingEffectComponent()
    {
        InteractionStarted += HandleInteractionStarted;
        InteractionCompleted += HandlenteractionCompleted;
    }

    private void HandleInteractionStarted(DurationBasedHoldInteractionComponent durationBasedHoldInteractionComponent, Entity owningEntity, TimeSpan time)
    {
        if (owningEntity.Tag == Entity.EntityTag.Player)
            lock (_lock)
            {
                _ringId = OverlayService.AddRing3dDisplay(owningEntity, Entity.Transform.Position, time);
                Logger.LogInformation("Added ring id: {_ringId}", _ringId);
            }
    }

    private void HandlenteractionCompleted(DurationBasedHoldInteractionComponent durationBasedHoldInteractionComponent, Entity owningEntity, bool succeed)
    {
        if (owningEntity.Tag == Entity.EntityTag.Player)
            lock (_lock)
                if (_ringId != null)
                {
                    Logger.LogInformation("Removed ring id: {_ringId}", _ringId);
                    OverlayService.RemoveRing3dDisplay(owningEntity, _ringId);
                    _ringId = null;
                }
    }
}