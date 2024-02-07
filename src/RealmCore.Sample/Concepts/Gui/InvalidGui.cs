using RealmCore.Server.Concepts.Gui;

namespace RealmCore.Sample.Concepts.Gui;

public sealed class InvalidGui : DxGui
{
    public InvalidGui(RealmPlayer player) : base(player, "invalidgui", false)
    {

    }
}
