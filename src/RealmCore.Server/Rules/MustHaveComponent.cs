namespace RealmCore.Server.Rules;

public sealed class MustHaveComponent<TComponent> : IElementRule
    where TComponent : Component
{
    public bool Check(Element element)
    {
        if(element is RealmPlayer player)
            return player.Components.HasComponent<TComponent>();
        return false;
    }
}
