using Size = System.Drawing.Size;
using Color = System.Drawing.Color;
using RealmCore.Resources.Assets.AssetsTypes;
using System.Security.Cryptography;

namespace RealmCore.BlazorGui.HudLayers;

public class SampleHudState
{
    public string String { get; set; } = "initialValue";
}

public class SampleHudLayer : HudLayer<SampleHudState>
{
    private readonly AssetsCollection _assetsCollection;
    private int? _imageId;
    private int? _id;
    public SampleHudLayer(AssetsCollection assetsCollection) : base(new())
    {
        _assetsCollection = assetsCollection;
    }

    protected override void Build(IHudBuilder builder, IHudBuilderContext context)
    {
        builder.Add(new RadarHudElement(new Vector2(100, 100), new Size(400, 400), new ImageHudElementContent(_assetsCollection.GetAsset<IRemoteImageAsset>("assets/map.jpg")), new Dictionary<int, IImageAsset>
        {
            [0] = _assetsCollection.GetAsset<IRemoteImageAsset>("assets/blip43.jpg"),
            [43] = _assetsCollection.GetAsset<IRemoteImageAsset>("assets/blip43.jpg")
        }, PositioningMode.Absolute));
        builder.Add(new TextHudElement(new CurrentFPSTextHudElementContent(), new Vector2(0, context.Bottom - 20), new Size(200, 20), font: BuildInFonts.Default, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center, positioningMode: PositioningMode.Absolute));
        var imageElement = builder.Add(new ImageHudElement(new Vector2(700, 700), new Size(64, 64), new ImageHudElementContent(_assetsCollection.GetAsset<IRemoteImageAsset>("assets/blip43.jpg"))));
        var element = builder.Add(new RectangleHudElement(new Vector2(context.Right - 400, 600), new Size(400, 20), Color.DarkBlue));
        _imageId = imageElement.Id;
        _id = element.Id;
        builder.Add(new TextHudElement(new ConstantTextHudElementContent("default font"), new Vector2(context.Right - 200, 600), new Size(200, 20), font: BuildInFonts.Default, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
        builder.Add(new TextHudElement(new ConstantTextHudElementContent("sans font"), new Vector2(context.Right - 400, 600), new Size(200, 20), font: BuildInFonts.Sans, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
        builder.Add(new TextHudElement(new ConstantTextHudElementContent("custom font"), new Vector2(context.Right - 600, 600), new Size(200, 20), font: _assetsCollection.GetFont("Better Together"), alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
        builder.Add(new TextHudElement(CreateStatePropertyTextHudElement(x => $"string = {x.String}"), new Vector2(context.Right - 400, 630), new Size(200, 20), font: BuildInFonts.Sans, alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center));
    }

    public void Resize()
    {
        SetSize(_id.Value, new Size(Random.Shared.Next(20, 40), Random.Shared.Next(20, 40)));
    }
    
    public void SetPosition()
    {
        SetPosition(_id.Value, new Vector2(Random.Shared.Next(20, 40), Random.Shared.Next(20, 40)));
    }

    private bool _visible = true;
    public void ToggleVisible()
    {
        _visible = !_visible;
        SetVisible(_id.Value, _visible);
    }

    private bool _content = false;
    public void ToggleContent()
    {
        _content = !_content;
        if (_content)
        {
            SetContent(_imageId.Value, new ImageHudElementContent(_assetsCollection.GetDynamicRemoteImage("assets/map.jpg")));
        }
        else
        {
            SetContent(_imageId.Value, new ImageHudElementContent(_assetsCollection.GetDynamicRemoteImage("assets/blip43.jpg")));
        }
    }

    public void Update()
    {
        UpdateState(x =>
        {
            x.String = Guid.NewGuid().ToString();
        });
    }
}
