﻿using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class TestShopGuiComponent : GuiComponent
{
    public TestShopGuiComponent() : base("shop", true)
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
    }
}