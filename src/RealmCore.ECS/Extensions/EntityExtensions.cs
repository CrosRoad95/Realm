namespace RealmCore.ECS.Extensions;

public static class EntityExtensions
{
    public static Entity WithName(this Entity entity, string name)
    {
        entity.AddComponent(new NameComponent(name));
        return entity;
    }
}
