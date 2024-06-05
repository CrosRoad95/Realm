using Size = System.Drawing.Size;
using Color = System.Drawing.Color;
using RealmCore.Resources.Overlay.Enums;

namespace RealmCore.Sample.HudLayers;

public class SampleHudLayer : HudLayer
{
    private readonly AssetsCollection _assetsCollection;

    public SampleHudLayer(AssetsCollection assetsCollection) : base(new())
    {
        _assetsCollection = assetsCollection;
    }

    protected override void Build(IHudBuilder<object> x, IHudBuilderContext context)
    {
        x.AddRectangle(new Vector2(context.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        x.AddText("foo bar", new Vector2(context.Right - 200, 600), new Size(200, 20), font: BuildInFonts.Default, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
        x.AddText("custom font", new Vector2(context.Right - 400, 600), new Size(200, 20), font: _assetsCollection.GetFont("Better Together.otf"), alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
    }
}
