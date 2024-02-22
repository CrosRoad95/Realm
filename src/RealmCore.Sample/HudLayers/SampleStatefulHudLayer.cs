using RealmCore.Resources.Overlay.Enums;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace RealmCore.Sample.HudLayers;

public class SampleHudState
{
    public string Text1 { get; set; }
    public string Text2 { get; set; }
}


public class SampleStatefulHudLayer : HudLayer<SampleHudState>
{
    private readonly AssetsCollection _assetsCollection;
    private readonly RealmPlayer _player;

    public SampleStatefulHudLayer(PlayerContext playerContext, AssetsCollection assetsCollection) : base()
    {
        _player = playerContext.Player;
        _assetsCollection = assetsCollection;
    }

    protected override SampleHudState? GetInitialState()
    {
        return new SampleHudState
        {
            Text1 = $"INITIAL STATE1 {_player.Name}",
            Text2 = $"INITIAL STATE2 {_player.Name}",
        };
    }

    protected override void Build(IHudBuilder<SampleHudState> x)
    {
        x.AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        x.AddText(x => x.Text1, new Vector2(x.Right - 200, 600), new Size(200, 20), font: "default", alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
        x.AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: _assetsCollection.GetFont("Better Together.otf"), alignX: HorizontalAlign.Center, alignY: VerticalAlign.Center);
    }

    public void Update()
    {
        UpdateState(x =>
        {
            x.Text1 = Guid.NewGuid().ToString()[..8];
        });
    }
}