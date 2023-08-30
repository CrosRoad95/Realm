using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class InvalidGuiComponent : DxGuiComponent
{
    public InvalidGuiComponent() : base("invalidgui", false)
    {

    }
}
