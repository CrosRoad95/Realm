

namespace RealmCore.Server.Components.Elements;

public class VehicleElementComponent : Vehicle, IElementComponent
{
    public VehicleElementComponent(ushort model, Vector3 position) : base(model, position)
    {
    }

    public Entity Entity { get; set; }
}
