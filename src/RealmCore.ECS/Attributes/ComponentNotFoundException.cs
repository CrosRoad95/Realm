namespace RealmCore.ECS.Attributes;

public class ComponentNotFoundException<T> : Exception
{
    public ComponentNotFoundException() : base($"Component '{typeof(T).Name}' not found")
    { }
}
