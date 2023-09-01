namespace RealmCore.ECS.Components;

public class NameComponent : Component
{
    public string Name { get; }
    public NameComponent(string name)
    {
        Name = name;
    }
}
