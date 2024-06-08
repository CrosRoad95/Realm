using Size = System.Drawing.Size;
using Color = System.Drawing.Color;

namespace RealmCore.BlazorGui.HudLayers;

public class SampleVehicleHudLayer : HudLayer
{
    private readonly AssetsCollection _assetsCollection;

    public SampleVehicleHudLayer(AssetsCollection assetsCollection) : base(new())
    {
        _assetsCollection = assetsCollection;
    }

    protected override void Build(IHudBuilder builder, IHudBuilderContext context)
    {
        builder.Add(new RectangleHudElement(new Vector2(context.Right - 400, 600), new Size(400, 20), Color.DarkBlue));
        builder.Add(new TextHudElement(new CurrentVehicleSpeedTextHudElementContent(), new Vector2(context.Right - 200, 600), new Size(200, 20), font: BuildInFonts.Default, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
        builder.Add(new TextHudElement(new ConstantTextHudElementContent("custom font"), new Vector2(context.Right - 400, 600), new Size(200, 20), font: _assetsCollection.GetFont("Better Together.otf"), alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
    }
}
