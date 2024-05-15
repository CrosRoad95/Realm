namespace RealmCore.Server.Modules.Vehicles.Access;

internal sealed class VehicleAccessControllerLogic : IHostedService
{
    private readonly IElementFactory _elementFactory;
    private readonly IVehiclesAccessService _vehicleAccessService;
    private readonly ILogger<VehicleAccessControllerLogic> _logger;

    public VehicleAccessControllerLogic(IElementFactory elementFactory, IVehiclesAccessService vehicleAccessService, ILogger<VehicleAccessControllerLogic> logger)
    {
        _elementFactory = elementFactory;
        _vehicleAccessService = vehicleAccessService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _elementFactory.ElementCreated += HandleElementCreated;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _elementFactory.ElementCreated -= HandleElementCreated;
        return Task.CompletedTask;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is not RealmVehicle vehicle)
            return;

        vehicle.CanEnter = HandleCanEnter;
        vehicle.CanExit = HandleCanExit;
    }

    private bool HandleCanEnter(Ped ped, Vehicle veh, byte seat)
    {
        try
        {
            var vehicle = (RealmVehicle)veh;
            if (!_vehicleAccessService.InternalCanEnter(ped, vehicle, seat, vehicle.AccessController))
                return false;

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogHandleError(ex);
            return false;
        }
    }

    private bool HandleCanExit(Ped ped, Vehicle veh, byte seat)
    {
        try
        {
            var vehicle = (RealmVehicle)veh;
            if (!_vehicleAccessService.InternalCanExit(ped, vehicle, seat, vehicle.AccessController))
                return false;

            if (ped is RealmPlayer realmPlayer)
            {
                realmPlayer.Admin.NoClip = false;
            }

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogHandleError(ex);
            return false;
        }
    }
}
