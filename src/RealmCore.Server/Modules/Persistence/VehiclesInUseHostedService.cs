namespace RealmCore.Server.Modules.Persistence;

internal sealed class VehiclesInUseHostedService : VehicleLifecycle, IHostedService
{
    private readonly VehiclesInUse _vehiclesInUse;

    public VehiclesInUseHostedService(IElementFactory elementFactory, VehiclesInUse vehiclesInUse) : base(elementFactory)
    {
        _vehiclesInUse = vehiclesInUse;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void VehicleLoaded(VehiclePersistenceFeature persistatnce, RealmVehicle vehicle)
    {
        _vehiclesInUse.TrySetActive(persistatnce.Id, vehicle);
    }
}
