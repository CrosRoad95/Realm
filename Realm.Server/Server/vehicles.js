const spawnForTestVehicle = createSpawn(new Vector3(-10, -10, 3), new Vector3(0, 0, 0), "spawnForTestVehicle");
const spawnForTestVehicle2 = createSpawn(new Vector3(-17, 10, 3), new Vector3(0, 0, 0), "spawnForTestVehicle2");
const testVehicle = createVehicle(404, spawnForTestVehicle)
Logger.information("spawned vehicle: {testVehicle}", testVehicle);

(async () => {
    try {
        let veh;
        if (!await isVehicleIdAvailiable("foo")) {
            veh = await createNewPersistantVehicle("foo", 404, spawnForTestVehicle2);
        }
        else {
            veh = await spawnPersistantVehicle("foo", spawnForTestVehicle2);
        }
        veh.isFrozen = false;

        const hasFuelComponent = veh.components.hasComponent(host.typeOf(VehicleFuelComponent));
        if (!hasFuelComponent)
            veh.components.addComponent(new VehicleFuelComponent(2, 20, 4, 2.5, "petrol"));

        const hasVehicleUpgradeComponent = veh.components.hasComponent(host.typeOf(VehicleUpgradeComponent));
        if (!hasVehicleUpgradeComponent)
            veh.components.addComponent(new VehicleUpgradeComponent());

        const hasMileageComponent = veh.components.hasComponent(host.typeOf(MileageCounterComponent));
        if (!hasMileageComponent)
            veh.components.addComponent(new MileageCounterComponent(2.5));

        if (veh.components.getComponent(host.typeOf(VehicleUpgradeComponent)).getUpgradesCount() == 0) {
            var seededFasterCarUpgrade = getVehicleUpgradeByName("fasterCar");
            veh.components.getComponent(host.typeOf(VehicleUpgradeComponent)).addUpgrade(seededFasterCarUpgrade)
            Logger.information("Added faster car upgrade from seed");
        }
        else {
            Logger.information("Test vehicle already have at least one upgrade.");
        }
    }
    catch (ex) {
        Logger.information("spawnPersistantVehicle ex: {exception}", ex.message);
    }
})();