namespace RealmCore.Server.Abstractions;

public abstract class VehicleLogic
{
    public VehicleLogic(IElementFactory elementFactory)
    {
        elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if(element is RealmVehicle vehicle)
        {
            vehicle.Persistance.Loaded += HandleLoaded;
        }
    }

    protected abstract void HandleLoaded(IVehiclePersistanceService persistatnce, RealmVehicle vehicle);
}
