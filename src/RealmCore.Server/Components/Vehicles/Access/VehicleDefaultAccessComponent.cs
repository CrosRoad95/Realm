namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleDefaultAccessComponent : VehicleAccessControllerComponent
{
    protected override bool CanEnter(Entity _1, Entity _2) => true;
}
