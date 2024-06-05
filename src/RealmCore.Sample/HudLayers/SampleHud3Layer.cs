using RealmCore.Resources.Overlay.Enums;

namespace RealmCore.Sample.HudLayers;

public class SampleHud3State
{
    public string Text { get; set; }
}

public class SampleHud3Layer : HudLayer<SampleHud3State>
{
    public SampleHud3Layer() : base(new SampleHud3State
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

    protected override void Build(IHudBuilder<SampleHud3State> x, IHudBuilderContext context)
    {
        //x.AddRectangle(new Vector2(context.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        x.AddText(x => x.Text, new Vector2(context.Right - 400, 600), new Size(200, 20), Color.Blue, scale: new Size(2, 2), font: BuildInFonts.Default, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center).AddShadow(new Vector2(2, 2));
    }
}
