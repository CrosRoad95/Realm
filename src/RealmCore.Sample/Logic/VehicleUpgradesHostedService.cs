using SlipeServer.Packets.Definitions.Entities.Structs;

namespace RealmCore.Sample.Logic;

public class VehicleUpgradesHostedService : IHostedService
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

    public VehicleUpgradesHostedService(VehicleUpgradesCollection vehicleUpgradeCollection)
    {
        vehicleUpgradeCollection.Add(1, new(new SampleUpgrade1()));
        vehicleUpgradeCollection.Add(2, new(new SampleUpgrade2()));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
