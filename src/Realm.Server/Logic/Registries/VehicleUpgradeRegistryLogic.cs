﻿using Realm.Domain.Registries;

namespace Realm.Server.Logic.Registries;

internal class VehicleUpgradeRegistryLogic
{
    public VehicleUpgradeRegistryLogic(VehicleUpgradeRegistry vehicleUpgradeRegistry)
    {
        vehicleUpgradeRegistry.AddUpgrade(1, new VehicleUpgradeRegistryEntry());
    }
}