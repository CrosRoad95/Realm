namespace RealmCore.Server.Modules.World.Triggers;

public sealed class MustBeVehicleRule : IElementRule
{
    public bool Check(Element element)
    {
        return element is Element;
    }
}
