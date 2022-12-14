using Realm.Domain.New;
using Realm.Interfaces.Server;

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

    private readonly List<Component> _components= new();
    private readonly IRPGServer _rpgServer;

    [ScriptMember("transform")]
    public Transform Transform { get; private set; }
    public IRPGServer Server => _rpgServer;
    public Entity(IRPGServer rpgServer, string name = "")
    {
        _rpgServer = rpgServer;
        Name = name;
        Transform = new Transform(this);
    }

    [ScriptMember("addComponent")]
    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        if(component.Entity != null)
        {
            throw new Exception("Component already attached to other entity");
        }
        component.Entity = this;
        _components.Add(component);
        return component;
    }
    
    [ScriptMember("getComponent", ScriptMemberFlags.ExposeRuntimeType)]
    public Component? GetComponent(Type type)
        => _components.Where(x => x.GetType() == type).FirstOrDefault();

    public override string ToString() => Name;
}
