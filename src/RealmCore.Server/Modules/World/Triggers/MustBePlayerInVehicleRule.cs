namespace RealmCore.Server.Modules.World.Triggers;

public sealed class MustBePlayerInVehicleAsDriver : IElementRule
{
    public bool Check(Element element)
    {
        if (element is RealmPlayer player)
        {
            return player.Vehicle != null;
        }
        return false;
    }
}
