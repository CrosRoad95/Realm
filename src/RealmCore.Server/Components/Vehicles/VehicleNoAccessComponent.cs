namespace RealmCore.Server.Components.Vehicles;

public class VehicleNoAccessComponent : VehicleAccessControllerComponent
{
    protected override void Load()
    {
        base.Load();
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = CanEnter;
    }

    protected override bool CanEnter(Ped ped, Vehicle vehicle) => false;

    public override void Dispose()
    {
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = null;
        base.Dispose();
    }
}
