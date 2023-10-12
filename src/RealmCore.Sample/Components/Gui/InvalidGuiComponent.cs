using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Sample.Components.Gui;

[ComponentUsage(false)]
public sealed class InvalidGuiComponent : DxGuiComponent
{
    public InvalidGuiComponent() : base("invalidgui", false)
    {

    }
}
