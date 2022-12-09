using Realm.Module.Scripting.Extensions;
using Realm.Module.Scripting.Functions;

namespace Realm.Server.Logic;

internal class RPGVehicleLogic
{
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly IRPGElementsFactory _rpgElementsFactory;

    public RPGVehicleLogic(EventScriptingFunctions eventFunctions, IRPGElementsFactory rpgElementsFactory)
    {
        _eventFunctions = eventFunctions;
        _rpgElementsFactory = rpgElementsFactory;
        _rpgElementsFactory.VehicleCreated += HandleVehicleCreated;
    }

    private void HandleVehicleCreated(RPGVehicle rpgVehicle)
    {
        rpgVehicle.Spawned += HandleSpawned;

        rpgVehicle.Destroyed += e =>
        {
            rpgVehicle.Spawned -= HandleSpawned;
        };
    }

    private async void HandleSpawned(RPGVehicle vehicle, RPGSpawn? spawn)
    {
        using var vehicleSpawnedEvent = new VehicleSpawnedEvent(vehicle, spawn);
        await _eventFunctions.InvokeEvent(vehicleSpawnedEvent);
    }
}
