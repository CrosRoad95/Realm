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

    protected override void Build(IHudBuilder<object> x)
    {
        x.AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        x.AddVehicleSpeed(new Vector2(x.Right - 200, 600), new Size(200, 20), font: "default", alignX: Resources.Overlay.Enums.HorizontalAlign.Center, alignY: VerticalAlign.Center);
        x.AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: _assetsCollection.GetFont("Better Together.otf"), alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
    }
}
