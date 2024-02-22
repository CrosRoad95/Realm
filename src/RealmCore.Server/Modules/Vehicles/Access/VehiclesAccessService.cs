namespace RealmCore.Server.Modules.Vehicles.Access;

public interface IVehiclesAccessService
{
    event Func<Ped, RealmVehicle, byte, bool>? CanEnter;
    event Func<Ped, RealmVehicle, byte, bool>? CanExit;
    event Action<Ped, RealmVehicle, byte, VehicleAccessController>? FailedToEnter;
    event Action<Ped, RealmVehicle, byte, VehicleAccessController>? FailedToExit;

    bool InternalCanExit(Ped ped, RealmVehicle vehicle, byte seat, VehicleAccessController? vehicleAccessController = null);
    internal bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat, VehicleAccessController? vehicleAccessController = null);
}

internal sealed class VehiclesAccessService : IVehiclesAccessService
{
    private readonly ILogger<VehiclesAccessService> _logger;

    public event Func<Ped, RealmVehicle, byte, bool>? CanEnter;
    public event Func<Ped, RealmVehicle, byte, bool>? CanExit;
    public event Action<Ped, RealmVehicle, byte, VehicleAccessController>? FailedToEnter;
    public event Action<Ped, RealmVehicle, byte, VehicleAccessController>? FailedToExit;

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

    public bool InternalCanExit(Ped ped, RealmVehicle vehicle, byte seat, VehicleAccessController? vehicleAccessController = null)
    {
        if (CanExit != null)
        {
            foreach (Func<Ped, RealmVehicle, byte, bool> handler in CanExit.GetInvocationList().Cast<Func<Ped, RealmVehicle, byte, bool>>())
            {
                if (handler.Invoke(ped, vehicle, seat))
                {
                    return true;
                }
            }
        }

        if (vehicleAccessController != null)
        {
            if (!vehicleAccessController.InternalCanExit(ped, vehicle, seat))
            {
                FailedToExit?.Invoke(ped, vehicle, seat, vehicleAccessController);
                return false;
            }
        }
        return true;
    }
}
