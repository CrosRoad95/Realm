using Discord;
using System.Runtime.Serialization;

namespace Realm.Server.Components;

[Serializable]
[NoDefaultScriptAccess]
public class ComponentSystem : ISerializable
{
    private Element _owner = default!;
    private ILogger _logger = default!;
    private readonly List<IElementComponent> _components = new();
    public event Action<ComponentSystem>? NotifyNotSavedState;

    public ComponentSystem(Element owner, ILogger logger)
    {
        _owner = owner;
        _logger = logger;
    }

    public ComponentSystem(SerializationInfo info, StreamingContext context)
    {
        _components = (List<IElementComponent>?)info.GetValue("Components", typeof(List<IElementComponent>)) ?? throw new SerializationException();
    }

    public void SetLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void SetOwner(Element element)
    {
        if (_owner != null)
            throw new Exception("Component system already have an owner.");
        _owner = element;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Components", _components);
    }

    [ScriptMember("addComponent")]
    public void AddComponent(IElementComponent component)
    {
        component.SetLogger(_logger);
        component.SetOwner(_owner);
        _components.Add(component);
        _logger.Verbose("Added component {elementComponentName}", component.Name);
    }
}
