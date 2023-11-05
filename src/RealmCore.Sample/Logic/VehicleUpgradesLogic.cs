using RealmCore.Server.Concepts.Interfaces;
using SlipeServer.Packets.Definitions.Entities.Structs;

namespace RealmCore.Sample.Logic;

public class VehicleUpgradesLogic
{
    private class SampleUpgrade1 : IVehicleHandlingModifier
    {
        public void Apply(VehicleHandlingContext context, HandlingDelegate next)
        {
            context.Modify((ref VehicleHandling vehicleHandling) =>
            {
                vehicleHandling.MaxVelocity += 50;
            });
            next(context);
        }
    }
    
    private class SampleUpgrade2 : IVehicleHandlingModifier
    {
        public void Apply(VehicleHandlingContext context, HandlingDelegate next)
        {
            context.Modify((ref VehicleHandling vehicleHandling) =>
            {
                vehicleHandling.MaxVelocity += 50;
            });
            next(context);
        }
    }

    public VehicleUpgradesLogic(VehicleUpgradeRegistry vehicleUpgradeRegistry)
    {
        vehicleUpgradeRegistry.AddUpgrade(1, new SampleUpgrade1());
        vehicleUpgradeRegistry.AddUpgrade(2, new SampleUpgrade2());
    }
}
