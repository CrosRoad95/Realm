using Size = System.Drawing.Size;
using Color = System.Drawing.Color;
using RealmCore.Resources.Overlay.Enums;

namespace RealmCore.Sample.HudLayers;

public class SampleVehicleHudLayer : HudLayer
{
    private readonly AssetsCollection _assetsCollection;

    public SampleVehicleHudLayer(AssetsCollection assetsCollection) : base(new())
    {
        _assetsCollection = assetsCollection;
    }

    protected override void Build(IHudBuilder<object> builder, IHudBuilderContext context)
    {
        builder.AddRectangle(new Vector2(context.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        builder.AddVehicleSpeed(new Vector2(context.Right - 200, 600), new Size(200, 20), font: "default", alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
        builder.AddText("custom font", new Vector2(context.Right - 400, 600), new Size(200, 20), font: _assetsCollection.GetFont("Better Together.otf"), alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
    }
}
