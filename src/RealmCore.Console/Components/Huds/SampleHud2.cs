using Size = System.Drawing.Size;
using Color = System.Drawing.Color;
using RealmCore.Resources.Overlay.Enums;
using RealmCore.Resources.Assets;
using RealmCore.Resources.Overlay.Interfaces;

namespace RealmCore.Console.Components.Huds;


public class SampleHud2State
{
    public string Text1 { get; set; }
    public string Text2 { get; set; }
}

[ComponentUsage(false)]
public class SampleHud2 : HudComponent<SampleHud2State>
{
    [Inject]
    private AssetsRegistry AssetsRegistry { get; set; } = default!;

    public SampleHud2() : base(new SampleHud2State
    {
        Text1 = "text1",
        Text2 = "text2"
    })
    {

    }

    protected override void Build(IHudBuilder<SampleHud2State> x)
    {
        x.AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue);

        x.AddText("foobar", new Vector2(100, 10), new Size(1,1), font: "default");
        x.AddText(x => x.Text2, new Vector2(100, 30), new Size(1,1));
        x.AddText(x => x.WithText(x => x.Text2).WithPosition(new Vector2(100, 50)));
        x.AddText(x => x.WithText("foo bar").WithPosition(new Vector2(100, 60)));
        x.AddText(x => x.WithText(x => $"Text1: {x.Text1.ToUpper()}").WithPosition(new Vector2(100, 80)));
        x.AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: AssetsRegistry.GetFont("Better Together.otf"), alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
    }

    public void Update()
    {
        UpdateState(x => x.Text1 = Guid.NewGuid().ToString());
        UpdateState(x => x.Text2 = Guid.NewGuid().ToString());
    }
}
