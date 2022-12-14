namespace Realm.Domain;

public abstract class Component
{
    [ScriptMember("entity")]
    public Entity? Entity { get; set; }

}
