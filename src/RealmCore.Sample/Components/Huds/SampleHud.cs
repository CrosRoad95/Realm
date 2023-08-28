using Size = System.Drawing.Size;
using Color = System.Drawing.Color;
using RealmCore.Resources.Overlay.Enums;
using RealmCore.Resources.Assets;
using RealmCore.Resources.Overlay.Interfaces;

namespace RealmCore.Console.Components.Huds;

public class SampleHud : HudComponent
{
    private readonly AssetsRegistry _assetsRegistry;

    public SampleHud(AssetsRegistry assetsRegistry) : base(new())
    {
        _assetsRegistry = assetsRegistry;
    }

    protected override void Build(IHudBuilder<object> x)
    {
        x.AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        x.AddText("foo bar", new Vector2(x.Right - 200, 600), new Size(200, 20), font: "default", alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
        x.AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: _assetsRegistry.GetFont("Better Together.otf"), alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
    }
}
