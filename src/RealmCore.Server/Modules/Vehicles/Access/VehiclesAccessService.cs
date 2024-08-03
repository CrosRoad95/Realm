namespace RealmCore.Server.Modules.Vehicles.Access;

public sealed class VehiclesAccessService
{
    private readonly ILogger<VehiclesAccessService> _logger;

    public event Func<Ped, RealmVehicle, byte, bool>? CanEnter;
    public event Func<Ped, RealmVehicle, byte, bool>? CanExit;
    public event Func<Ped, RealmVehicle, byte, bool>? CanNotEnter;
    public event Func<Ped, RealmVehicle, byte, bool>? CanNotExit;
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
        
        if (CanNotEnter != null)
        {
            foreach (Func<Ped, RealmVehicle, byte, bool> handler in CanNotEnter.GetInvocationList().Cast<Func<Ped, RealmVehicle, byte, bool>>())
            {
                if (handler.Invoke(ped, vehicle, seat))
                {
                    return false;
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
        
        if (CanNotExit != null)
        {
            foreach (Func<Ped, RealmVehicle, byte, bool> handler in CanNotExit.GetInvocationList().Cast<Func<Ped, RealmVehicle, byte, bool>>())
            {
                if (handler.Invoke(ped, vehicle, seat))
                {
                    return false;
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
