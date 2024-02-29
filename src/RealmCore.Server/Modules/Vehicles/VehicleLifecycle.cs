namespace RealmCore.Server.Modules.Vehicles;

public abstract class VehicleLifecycle
{
    public VehicleLifecycle(IElementFactory elementFactory)
    {
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if (element is RealmVehicle vehicle)
        {
            vehicle.Persistence.Loaded += HandleLoaded;
        }
    }

    protected abstract void HandleLoaded(IVehiclePersistenceFeature persistatnce, RealmVehicle vehicle);
}
