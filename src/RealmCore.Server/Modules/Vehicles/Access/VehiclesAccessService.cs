namespace RealmCore.Server.Modules.Vehicles.Access;

public interface IVehiclesAccessService
{
    event Func<Ped, RealmVehicle, byte, bool>? CanEnter;
    event Action<Ped, RealmVehicle, byte, VehicleAccessController>? FailedToEnter;

    internal bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat, VehicleAccessController? vehicleAccessControllerComponent = null);
}

internal sealed class VehiclesAccessService : IVehiclesAccessService
{
    private readonly ILogger<VehiclesAccessService> _logger;

    public event Func<Ped, RealmVehicle, byte, bool>? CanEnter;
    public event Action<Ped, RealmVehicle, byte, VehicleAccessController>? FailedToEnter;

    public VehiclesAccessService(ILogger<VehiclesAccessService> logger)
    {
        _logger = logger;
    }

    public bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat, VehicleAccessController? vehicleAccessController = null)
    {
        if (CanEnter != null)
        {
            foreach (Func<Ped, RealmVehicle, byte, bool> handler in CanEnter.GetInvocationList().Cast<Func<Ped, RealmVehicle, byte, bool>>())
            {
                if (handler.Invoke(ped, vehicle, seat))
                {
                    return true;
                }
            }
        }

        if (vehicleAccessController != null)
        {
            if (!vehicleAccessController.InternalCanEnter(ped, vehicle, seat))
            {
                FailedToEnter?.Invoke(ped, vehicle, seat, vehicleAccessController);
                return false;
            }
        }
        return true;
    }
}
