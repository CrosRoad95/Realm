using RealmCore.Resources.Overlay.Interfaces;

namespace RealmCore.Resources.Overlay;

internal sealed class HudBuilderContext : IHudBuilderContext
{
    private readonly Vector2 _screenSize;
    public float Right => _screenSize.X;
    public float Bottom => _screenSize.Y;

    public HudBuilderContext(Vector2 screenSize)
    {
        _screenSize = screenSize;
    }
}
