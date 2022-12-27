using Microsoft.AspNetCore.Identity;
using Realm.Console.Data;
using Realm.Persistance.Data;

namespace Realm.Console.Components.Gui;

public sealed class TestWindowComponent : GuiComponent
{
    public TestWindowComponent() : base("test", true)
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
    }
}
