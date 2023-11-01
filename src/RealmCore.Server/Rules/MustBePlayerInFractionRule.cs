namespace RealmCore.Server.Rules;

public sealed class MustBePlayerInFractionRule : IElementRule
{
    private readonly int _fractionId;

    public MustBePlayerInFractionRule(int fractionId)
    {
        _fractionId = fractionId;
    }

    public bool Check(Element element)
    {
        if(element is RealmPlayer player)
        {
            return player.Components.ComponentsLists.OfType<FractionMemberComponent>().Where(x => x.FractionId == _fractionId).Any();
        }
        return false;
    }
}
