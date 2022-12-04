const spawnForTestVehicle = createSpawn(new Vector3(-10, -10, 3), new Vector3(0, 0, 0), "spawnForTestVehicle");
const spawnForTestVehicle2 = createSpawn(new Vector3(-20, 10, 3), new Vector3(0, 0, 0), "spawnForTestVehicle2");
const testVehicle = createVehicle(404, spawnForTestVehicle)
Logger.information("spawned vehicle: {testVehicle}", testVehicle);

var fasterCar = new VehicleUpgrade()
fasterCar.maxVelocity = new UpgradeDescription(100, 2);
fasterCar.engineAcceleration = new UpgradeDescription(100, 2);

(async () => {
    try {
        let veh;
        if (!await isVehicleIdAvailiable("foo")) {
            veh = await createNewPersistantVehicle("foo", 404, spawnForTestVehicle2);
        }
        else {
            veh = await spawnPersistantVehicle("foo", spawnForTestVehicle2);
        }
        veh.addUpgrade(fasterCar)
        veh.isFrozen = false;

        const hasFuelComponent = veh.components.hasComponent(host.typeOf(VehicleFuelComponent));
        if (!hasFuelComponent)
            veh.components.addComponent(new VehicleFuelComponent(2, 20, 4, 2.5, "petrol"));

        const hasMileageComponent = veh.components.hasComponent(host.typeOf(MileageCounterComponent));
        if (!hasMileageComponent)
            veh.components.addComponent(new MileageCounterComponent(2.5));
    }
    catch (ex) {
        Logger.information("spawnPersistantVehicle ex: {exception}", ex.message);
    }
})();