using Realm.Resources.Assets.Interfaces;
using Realm.Resources.Assets;
using System.Drawing;
using Realm.Resources.Overlay.Interfaces;
using Size = System.Drawing.Size;
using Color = System.Drawing.Color;

namespace Realm.Console.Components.Huds;

public class SampleHud : HudComponent
{
    [Inject]
    private AssetsRegistry AssetsRegistry { get; set; } = default!;

    public SampleHud() : base(new())
    {

    }

    protected override void Build(IHudBuilder<object> x)
    {
        x.AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        x.AddText("foo bar", new Vector2(x.Right - 200, 600), new Size(200, 20), font: "default", alignX: "center", alignY: "center");
        x.AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: AssetsRegistry.GetFont("Better Together.otf"), alignX: "center", alignY: "center");
    }
}
