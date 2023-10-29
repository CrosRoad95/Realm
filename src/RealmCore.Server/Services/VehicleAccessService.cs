namespace RealmCore.Server.Services;

internal sealed class VehicleAccessService : IVehicleAccessService
{
    private readonly IEntityEngine _entityEngine;
    private readonly ILogger<VehicleAccessService> _logger;

    public event Func<Entity, Entity, bool>? CanEnter;
    public event Action<Entity, Entity, VehicleAccessControllerComponent>? FailedToEnter;

    public VehicleAccessService(IEntityEngine entityEngine, ILogger<VehicleAccessService> logger)
    {
        _entityEngine = entityEngine;
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
