namespace RealmCore.Server.Rules;

public sealed class MustHaveComponentRule<TComponent> : IElementRule
    where TComponent : Component
{
    public bool Check(Element element)
    {
        if(element is RealmPlayer player)
            return player.HasComponent<TComponent>();
        return false;
    }
}
