namespace RealmCore.ECS.Components;

public class NameComponent : IComponent
{
    public string Name { get; }
    public Entity? Entity { get; set; }

    public NameComponent(string name)
    {
        Name = name;
    }

    public void Attach() { }

    public void Detach() { }

    public void Dispose() { }
}
