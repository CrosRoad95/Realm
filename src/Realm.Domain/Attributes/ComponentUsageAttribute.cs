namespace Realm.Domain.Attributes;

public class ComponentUsageAttribute : Attribute
{
    public bool AllowMultiple { get; }
    public ComponentUsageAttribute(bool allowMultiple = false)
    {
        AllowMultiple = allowMultiple;
    }
}
