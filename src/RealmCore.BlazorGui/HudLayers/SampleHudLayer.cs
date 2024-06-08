using Size = System.Drawing.Size;
using Color = System.Drawing.Color;

namespace RealmCore.BlazorGui.HudLayers;

public class SampleHudState
{
    public string String { get; set; }
}

public class SampleHudLayer : HudLayer<SampleHudState>
{
    private readonly AssetsCollection _assetsCollection;

    public SampleHudLayer(AssetsCollection assetsCollection) : base(new())
    {
        _assetsCollection = assetsCollection;
    }

    protected override void Build(IHudBuilder builder, IHudBuilderContext context)
    {
        builder.Add(new RectangleHudElement(new Vector2(context.Right - 400, 600), new Size(400, 20), Color.DarkBlue));
        builder.Add(new TextHudElement(new ConstantTextHudElementContent("default font"), new Vector2(context.Right - 200, 600), new Size(200, 20), font: BuildInFonts.Default, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
        builder.Add(new TextHudElement(new ConstantTextHudElementContent("sans font"), new Vector2(context.Right - 400, 600), new Size(200, 20), font: BuildInFonts.Sans, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
        builder.Add(new TextHudElement(CreateStatePropertyTextHudElement(x => $"string = {x.String}"), new Vector2(context.Right - 400, 630), new Size(200, 20), font: BuildInFonts.Sans, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
    }

    public void Update()
    {
        UpdateState(x =>
        {
            x.String = Guid.NewGuid().ToString();
        });
    }
}
