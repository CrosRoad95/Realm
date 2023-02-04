namespace Realm.Server.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandNameAttribute : Attribute
{
    public string Name { get; }

    public CommandNameAttribute(string name)
    {
        Name = name;
    }
}
