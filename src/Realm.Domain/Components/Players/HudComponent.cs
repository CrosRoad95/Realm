﻿using Realm.Domain.Concepts;
using Realm.Domain.Interfaces;
using Realm.Resources.Overlay.Interfaces;
using SlipeServer.Server.Elements;

namespace Realm.Domain.Components.Players;

[ComponentUsage(true)]
public abstract class HudComponent<TState> : Component where TState : class
{
    [Inject]
    private OverlayService OverlayService { get; set; } = default!;

    private readonly string _id = Guid.NewGuid().ToString();
    private Hud<TState> _hud = default!;
    private readonly TState _defaultState;
    private readonly Vector2? _offset;
    private bool _visible;

    public bool Visible { get => _visible; set
        {
            if (_visible != value)
            {
                _visible = value;
                _hud.SetVisible(true);
            }
        }
    }

    public Vector2 Position { get => _hud.Position; set => _hud.Position = value; }

    public HudComponent(TState defaultState, Vector2? offset = null)
    {
        _defaultState = defaultState;
        _offset = offset;
    }

    protected override void Load()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        List<(int, PropertyInfo)> dynamicHudComponents = new();

        var HandleDynamicHudComponentAdded = (int id, PropertyInfo propertyInfo) =>
        {
            dynamicHudComponents.Add((id, propertyInfo));
        };

        OverlayService.CreateHud(playerElementComponent.Player, _id, e =>
        {
            e.DynamicHudComponentAdded += HandleDynamicHudComponentAdded;
            Build(e);
            e.DynamicHudComponentAdded -= HandleDynamicHudComponentAdded;
        }, playerElementComponent.ScreenSize, _offset, _defaultState);
        _hud = new Hud<TState>(_id, playerElementComponent.Player, OverlayService, _offset, _defaultState, dynamicHudComponents);
        Visible = true;
    }

    public void UpdateState(Action<TState> callback)
    {
        _hud.UpdateState(callback);
    }

    protected abstract void Build(IHudBuilder<TState> hudBuilderCallback);

    public override void Dispose()
    {
        var playerElementComponent = Entity.GetRequiredComponent<PlayerElementComponent>();
        OverlayService.RemoveHud(playerElementComponent.Player, _id);
    }
}

[ComponentUsage(true)]
public abstract class HudComponent : HudComponent<object>
{
    public HudComponent(Vector2? offset = null) : base(new(), offset)
    {

    }
}