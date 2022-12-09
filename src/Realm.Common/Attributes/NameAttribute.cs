namespace Realm.Common.Attributes;

public class NameAttribute : Attribute
{
    public string Name { get; set; }
    public NameAttribute(string name)
    {
        Name = name;
    }
}
