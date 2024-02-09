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

    public VehicleUpgradesLogic(VehicleUpgradeCollection vehicleUpgradeCollection)
    {
        vehicleUpgradeCollection.Add(1, new(new SampleUpgrade1()));
        vehicleUpgradeCollection.Add(2, new(new SampleUpgrade2()));
    }
}
