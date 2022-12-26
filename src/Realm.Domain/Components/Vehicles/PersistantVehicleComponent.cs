namespace Realm.Domain.Components.Vehicles;

public class PersistantVehicleComponent : Component
{
    private readonly string _id;

    public string Id => _id;
    public PersistantVehicleComponent(string id)
    {
        _id = id;
    }
}
