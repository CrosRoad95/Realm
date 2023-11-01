namespace RealmCore.Server.Rules;

public sealed class MustBeVehicleRule : IElementRule
{
    public bool Check(Element element)
    {
        return element is Element;
    }
}
