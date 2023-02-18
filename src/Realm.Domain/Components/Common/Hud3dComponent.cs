using Realm.Resources.Assets;
using Realm.Resources.Overlay.Interfaces;
using SlipeServer.Server.Elements;

namespace Realm.Domain.Components.Common;

public class Hud3dComponent<TState> : Component where TState : class
{
    [Inject]
    private OverlayService OverlayService { get; set; } = default!;

    private static int _idCounter = 0;
    private readonly int _id = _idCounter++;
    private readonly Action<IHudBuilder<TState>> _hudBuilderCallback;

    public Hud3dComponent(Action<IHudBuilder<TState>> hudBuilderCallback, TState defaultState)
    {
        _hudBuilderCallback = hudBuilderCallback;
    }

    protected override void Load()
    {
        OverlayService.CreateHud3d(_id.ToString(), _hudBuilderCallback, Entity.Transform.Position);
        base.Load();
    }
}
