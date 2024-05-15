
namespace RealmCore.Server.Modules.Persistence;

internal sealed class VehiclesInUseHostedService : VehicleLifecycle, IHostedService
{
    private readonly IVehiclesInUse _activeVehicles;

    public VehiclesInUseHostedService(IElementFactory elementFactory, IVehiclesInUse activeVehicles) : base(elementFactory)
    {
        _activeVehicles = activeVehicles;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void VehicleLoaded(IVehiclePersistenceFeature persistatnce, RealmVehicle vehicle)
    {
        _activeVehicles.TrySetActive(persistatnce.Id, vehicle);
    }
}
