namespace RealmCore.Server.Modules.World.Triggers;

public sealed class MustBePlayerRule : IElementRule
{
    public bool Check(Element element)
    {
        return element is Player;
    }
}
