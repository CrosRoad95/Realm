namespace Realm.Server.Factories;

public class EntityElementFactory
{
    private readonly IInternalRPGServer _internalRPGServer;

    public EntityElementFactory(IInternalRPGServer internalRPGServer)
    {
        _internalRPGServer = internalRPGServer;
    }

    public Entity CreateVehicle(ushort model, Vector3 position, Vector3 rotation)
    {
        var vehicle = _internalRPGServer.ECS.CreateEntity("vehicle");
        vehicle.AddComponent(new VehicleElementComponent(new Vehicle(model, position)));
        vehicle.Transform.Position = position;
        vehicle.Transform.Rotation = rotation;
        return vehicle;
    }
}
