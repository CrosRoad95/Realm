namespace RealmCore.Server.Rules;

public sealed class MustBePlayerRule : IElementRule
{
    public bool Check(Element element)
    {
        return element is Player;
    }
}
