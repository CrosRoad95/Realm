namespace Realm.Server.Components;

[NoDefaultScriptAccess]
public class ComponentSystem
{
    private readonly Element _owner;
    private readonly ILogger _logger;
    private readonly List<IComponent> _components = new();

    public ComponentSystem(Element owner, ILogger logger)
    {
        _owner = owner;
        _logger = logger;
    }

    [ScriptMember("addComponent")]
    public void AddComponent(IComponent component)
    {
        component.SetLogger(_logger);
        component.SetOwner(_owner);
        _components.Add(component);
    }
}
