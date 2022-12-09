namespace Realm.Server.Logic;

internal class RPGVehicleLogic
{
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly IRPGElementsFactory _rpgElementsFactory;

    public RPGVehicleLogic(EventScriptingFunctions eventFunctions, IRPGElementsFactory rpgElementsFactory)
    {
        _eventFunctions = eventFunctions;
        _rpgElementsFactory = rpgElementsFactory;
        _rpgElementsFactory.VehicleCreated += RPGElementsFactory_VehicleCreated;
    }

    private void RPGElementsFactory_VehicleCreated(RPGVehicle rpgVehicle)
    {
        rpgVehicle.Spawned += RPGVehicle_Spawned;

        rpgVehicle.Destroyed += e =>
        {
            rpgVehicle.Spawned -= RPGVehicle_Spawned;
        };
    }

    private async void RPGVehicle_Spawned(RPGVehicle vehicle, RPGSpawn? spawn)
    {
        using var vehicleSpawnedEvent = new VehicleSpawnedEvent(vehicle, spawn);
        await _eventFunctions.InvokeEvent(vehicleSpawnedEvent);
    }
}
