namespace RealmCore.Server.Rules;

public sealed class MustBePlayerOnFootOnlyRule : IElementRule
{
    public bool Check(Element element)
    {
        if(element is RealmPlayer player)
        {
            if (player.Vehicle != null)
                return false;

            if (player.HasJetpack)
                return false;

            if (player.VehicleAction != VehicleAction.None)
                return false;
            return true;
        }
        return false;
    }
}
