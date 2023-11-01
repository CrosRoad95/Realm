namespace RealmCore.Server.Components;

public class NameComponent : IComponent
{
    public string Name { get; }
    public Element Element { get; set; }

    public NameComponent(string name)
    {
        Name = name;
    }
}
