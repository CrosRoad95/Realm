namespace RealmCore.Server.Modules.Vehicles;

public abstract class VehicleLifecycle<TVehicle> where TVehicle : RealmVehicle
{
    public VehicleLifecycle(IElementFactory elementFactory)
    {
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if (element is TVehicle vehicle)
        {
            VehicleCreated(vehicle);
            vehicle.Persistence.Loaded += HandleLoaded;
            vehicle.Destroyed += HandleDestroyed;
        }
    }

    private void HandleLoaded(IVehiclePersistenceFeature persistatnce, RealmVehicle vehicle)
    {
        VehicleLoaded(persistatnce, (TVehicle)vehicle);
    }

    private void HandleDestroyed(Element element)
    {
        if (element is TVehicle vehicle)
        {
            vehicle.Persistence.Loaded -= HandleLoaded;
            vehicle.Destroyed -= HandleDestroyed;
            VehicleDestroyed(vehicle);
        }
    }

    protected virtual void VehicleCreated(TVehicle vehicle) { }
    protected virtual void VehicleDestroyed(TVehicle vehicle) { }
    protected virtual void VehicleLoaded(IVehiclePersistenceFeature persistatnce, TVehicle vehicle) { }
}

public abstract class VehicleLifecycle : VehicleLifecycle<RealmVehicle>
{
    protected VehicleLifecycle(IElementFactory elementFactory) : base(elementFactory)
    {
    }
}
