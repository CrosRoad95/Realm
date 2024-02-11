namespace RealmCore.Server.Modules.World.Triggers;

public sealed class MustBePlayerInVehicleAsDriverRule : IElementRule
{
    public bool Check(Element element)
    {
        if (element is RealmPlayer player)
        {
            if(player.Vehicle != null)
            {
                if (player.Vehicle.Driver == player)
                    return true;
            }
        }
        return false;
    }
}
