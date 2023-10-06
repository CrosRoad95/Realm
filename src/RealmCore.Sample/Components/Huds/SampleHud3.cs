using RealmCore.Resources.Overlay.Enums;
using RealmCore.Resources.Overlay.Interfaces;
using System.Drawing;

namespace RealmCore.Sample.Components.Huds;

public class SampleHud3State
{
    public string Text { get; set; }
}

public class SampleHud3 : HudComponent<SampleHud3State>
{
    public SampleHud3() : base(new SampleHud3State
    {
        Text = "initial value"
    })
    {
    }

    private int i = 0;
    public void Update()
    {
        UpdateState(x =>
        {
            x.Text = $"new {++i}";
        });
    }

    protected override void Build(IHudBuilder<SampleHud3State> x)
    {
        //x.AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        x.AddText(x => x.Text, new Vector2(x.Right - 400, 600), new Size(200, 20), Color.Blue, scale: new Size(2, 2), font: "default", alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center).AddShadow(new Vector2(2,2));
    }
}
