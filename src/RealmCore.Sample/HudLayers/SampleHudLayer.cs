using Size = System.Drawing.Size;
using Color = System.Drawing.Color;

namespace RealmCore.Sample.HudLayers;

public class SampleHudLayer : HudLayer
{
    private readonly AssetsRegistry _assetsRegistry;

    public SampleHudLayer(AssetsRegistry assetsRegistry) : base(new())
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
