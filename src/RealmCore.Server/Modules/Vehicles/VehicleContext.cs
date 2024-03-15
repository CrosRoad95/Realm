namespace RealmCore.Server.Modules.Vehicles;

public sealed class VehicleContext
{
    private RealmVehicle? _vehicle;

    public RealmVehicle Vehicle { get => _vehicle ?? throw new InvalidVehicleContextException(); internal set => _vehicle = value; }

    public void Set(RealmVehicle element)
    {
        if (Vehicle != null)
            throw new InvalidOperationException("Vehicle already set");

        Vehicle = element;
    }
}
