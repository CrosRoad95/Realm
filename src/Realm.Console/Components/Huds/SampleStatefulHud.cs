﻿using Realm.Resources.Assets;
using Realm.Resources.Assets.Interfaces;
using Realm.Resources.Overlay.Interfaces;
using System.Drawing;

namespace Realm.Console.Components.Huds;

public class SampleHudState
{
    public string Text1 { get; set; }
    public string Text2 { get; set; }
}


public class SampleStatefulHud : HudComponent<SampleHudState>
{
    [Inject]
    private AssetsRegistry AssetsRegistry { get; set; } = default!;

    public SampleStatefulHud(SampleHudState defaultState) : base(defaultState)
    {

    }

    protected override void Build(IHudBuilder<SampleHudState> x)
    {
        x.AddRectangle(new Vector2(x.Right - 400, 600), new Size(400, 20), Color.DarkBlue);
        x.AddText(x => x.Text1, new Vector2(x.Right - 200, 600), new Size(200, 20), font: "default", alignX: "center", alignY: "center");
        x.AddText("custom font", new Vector2(x.Right - 400, 600), new Size(200, 20), font: AssetsRegistry.GetFont("Better Together.otf"), alignX: "center", alignY: "center");
    }
}