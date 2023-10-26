namespace RealmCore.Server.Services;

internal sealed class VehicleAccessService : IVehicleAccessService
{
    private readonly IEntityEngine _ecs;
    private readonly ILogger<VehicleAccessService> _logger;

    public event Func<Entity, Entity, bool>? CanEnter;
    public event Action<Entity, Entity, VehicleAccessControllerComponent>? FailedToEnter;

    public VehicleAccessService(IEntityEngine ecs, ILogger<VehicleAccessService> logger)
    {
        _ecs = ecs;
        _logger = logger;
    }

    public bool InternalCanEnter(Entity pedEntity, Entity vehicleEntity, VehicleAccessControllerComponent? vehicleAccessControllerComponent = null)
    {
        if (CanEnter != null)
        {
            foreach (Func<Entity, Entity, bool> handler in CanEnter.GetInvocationList().Cast<Func<Entity, Entity, bool>>())
            {
                if (handler.Invoke(pedEntity, vehicleEntity))
                {
                    return true;
                }
            }
        }

        if (vehicleAccessControllerComponent != null)
        {
            if (!vehicleAccessControllerComponent.InternalCanEnter(pedEntity, vehicleEntity))
            {
                FailedToEnter?.Invoke(pedEntity, vehicleEntity, vehicleAccessControllerComponent);
                return false;
            }
        }
        return true;
    }
}
