using System.Security.Principal;

namespace Realm.Server.Concepts;

[Serializable]
[NoDefaultScriptAccess]
public class ComponentSystem : ISerializable
{
    private readonly List<IElementComponent> _components = new();
    private Element _owner = default!;

    public event Action<IElementComponent>? ComponentAdded;
    public ComponentSystem()
    {
    }

    public ComponentSystem(SerializationInfo info, StreamingContext context)
    {
        _components = (List<IElementComponent>?)info.GetValue("Components", typeof(List<IElementComponent>)) ?? throw new SerializationException();
    }

    public void SetOwner(Element element)
    {
        if (_owner != null)
            throw new Exception("Component system already have an owner.");
        _owner = element;
        foreach (var component in _components)
        {
            component.SetOwner(_owner);
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Components", _components);
    }

    [ScriptMember("addComponent")]
    public void AddComponent(IElementComponent component)
    {
        component.SetOwner(_owner);
        _components.Add(component);
        ComponentAdded?.Invoke(component);
    }

    [ScriptMember("hasComponent")]
    public bool HasComponent(Type type)
    {
        return _components.Where(x => x.GetType() == type).Any();
    }

    [ScriptMember("getComponent", ScriptMemberFlags.ExposeRuntimeType)]
    public IElementComponent? GetComponent(Type type)
    {
        return _components.Where(x => x.GetType() == type).FirstOrDefault();
    }

    [ScriptMember("getComponents")]
    public object GetComponents(Type type)
    {
        return _components.Where(x => x.GetType() == type).ToArray().ToScriptArray();
    }

    [ScriptMember("getComponents")]
    public object GetComponentsByName(string name)
    {
        return _components.Where(x => x.Name == name).ToArray().ToScriptArray();
    }

    [ScriptMember("hasComponentByName")]
    public bool HasComponent(string name)
    {
        return _components.Where(x => x.Name == name).Any();
    }

    [ScriptMember("countComponents")]
    public int CountComponent(Type type)
    {
        return _components.Where(x => x.GetType() == type).Count();
    }

    public static ComponentSystem CreateFromString(string str)
    {
        return JsonConvert.DeserializeObject<ComponentSystem>(str, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
        }) ?? throw new JsonSerializationException("Failed to deserialize ComponentSystem");
    }
}
