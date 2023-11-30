namespace RealmCore.Server.Rules;

public sealed class MustNotHaveComponentRule<TComponent> : IElementRule
    where TComponent : IComponent
{
    public bool Check(Element element)
    {
        if (element is RealmPlayer player)
            return !player.HasComponent<TComponent>();
        return true;
    }
}
