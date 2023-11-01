namespace RealmCore.Server.Services;

internal sealed class VehicleAccessService : IVehicleAccessService
{
    private readonly ILogger<VehicleAccessService> _logger;

    public event Func<Ped, RealmVehicle, byte, bool>? CanEnter;
    public event Action<Ped, RealmVehicle, byte, VehicleAccessControllerComponent>? FailedToEnter;

    public VehicleAccessService(ILogger<VehicleAccessService> logger)
    {
        _logger = logger;
    }

    public bool InternalCanEnter(Ped ped, RealmVehicle vehicle, byte seat, VehicleAccessControllerComponent? vehicleAccessControllerComponent = null)
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

        if (vehicleAccessControllerComponent != null)
        {
            if (!vehicleAccessControllerComponent.InternalCanEnter(ped, vehicle, seat))
            {
                FailedToEnter?.Invoke(ped, vehicle, seat, vehicleAccessControllerComponent);
                return false;
            }
        }
        return true;
    }
}
