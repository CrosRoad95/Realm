using Realm.Domain.New;
using Realm.Interfaces.Server;
using Realm.Module.Scripting.Extensions;

namespace Realm.Domain;

[NoDefaultScriptAccess]
public class Entity
{
    [ScriptMember("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [ScriptMember("tag")]
    public string Tag { get; set; } = "";
    [ScriptMember("name")]
    public string Name { get; set; } = "";

    private readonly List<Component> _components = new();
    private readonly IRPGServer _rpgServer;

    [ScriptMember("transform")]
    public Transform Transform { get; private set; }
    public IRPGServer Server => _rpgServer;


    public event Action<Entity>? Destroyed;

    public Entity(IRPGServer rpgServer, ServicesComponent servicesComponent, string name = "")
    {
        _rpgServer = rpgServer;
        Name = name;
        Transform = new Transform(this);
        AddComponent(servicesComponent);
    }

    [ScriptMember("addComponent")]
    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        if (component.Entity != null)
        {
            throw new Exception("Component already attached to other entity");
        }
        component.Entity = this;
        _components.Add(component);
        Task.Run(component.Load);
        return component;
    }

    [ScriptMember("getComponent", ScriptMemberFlags.ExposeRuntimeType)]
    public Component? GetComponent(Type type)
        => _components.Where(x => x.GetType() == type).FirstOrDefault();

    [ScriptMember("getComponents", ScriptMemberFlags.ExposeRuntimeType)]
    public object GetComponents() => _components.ToArray().ToScriptArray();

    #region Internal
    public TComponent? InternalGetComponent<TComponent>()
        => _components.OfType<TComponent>().FirstOrDefault();

    public TComponent InternalGetRequiredComponent<TComponent>()
        => _components.OfType<TComponent>().First();

    public List<Component> InternalGetComponents() => _components;

    public T GetRequiredService<T>() =>
        InternalGetRequiredComponent<ServicesComponent>().GetRequiredService<T>();
    #endregion

    public virtual void Destroy()
    {
        Destroyed?.Invoke(this);
    }

    public override string ToString() => Name;
}
