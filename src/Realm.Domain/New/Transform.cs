namespace Realm.Domain.New;

[NoDefaultScriptAccess]
public sealed class Transform
{
    [ScriptMember("entity", ScriptAccess.ReadOnly)]
    public Entity Entity { get; private set; }
    [ScriptMember("position")]
    public Vector3 Position { get; set; }
    [ScriptMember("rotation")]
    public Vector3 Rotation { get; set; }
    [ScriptMember("interior")]
    public byte Interior { get; set; }
    [ScriptMember("dimension")]
    public short Dimension { get; set; }

    public Transform(Entity entity)
    {
        Entity = entity;
    }
}
