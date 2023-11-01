namespace RealmCore.Server.Rules;

public sealed class MustNotHaveComponent<TComponent> : IElementRule
    where TComponent : IComponent
{
    public bool Check(Element element)
    {
        if (element is RealmPlayer player)
            return !player.Components.HasComponent<TComponent>();
        return true;
    }
}
