namespace RealmCore.Server.Modules.Vehicles;

public sealed class VehicleContext
{
    public RealmVehicle Vehicle { get; internal set; }

    public void Set(RealmVehicle realmVehicle)
    {
        if (Vehicle != null)
            throw new InvalidOperationException();

        Vehicle = realmVehicle;
    }
}
