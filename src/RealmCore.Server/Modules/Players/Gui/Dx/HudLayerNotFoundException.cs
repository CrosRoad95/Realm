namespace RealmCore.Server.Modules.Players.Gui.Dx;

public sealed class HudLayerNotFoundException : Exception
{
    public HudLayerNotFoundException(Type type) : base($"Hud of type {type} not found.")
    {

    }
}
