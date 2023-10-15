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

    // TODO: Refactor, remove out arguments
    public bool InternalCanEnter(Ped ped, Vehicle vehicle, out Entity? pedEntity, out Entity? vehicleEntity)
    {
        if(!_ecs.TryGetEntityByPed(ped, out pedEntity) || !_ecs.TryGetByElement(vehicle, out vehicleEntity))
        {
            using var _ = _logger.BeginElement(ped);
            _logger.LogWarning("Player/ped attempted to enter enter vehicle that has no entity.");
            pedEntity = null!;
            vehicleEntity = null!;
            return false;
        }

        if (pedEntity == null || vehicleEntity == null)
            return false;

        if (CanEnter == null)
            return false;

        foreach (Func<Entity, Entity, bool> handler in CanEnter.GetInvocationList().Cast<Func<Entity, Entity, bool>>())
        {
            if (!handler.Invoke(pedEntity, vehicleEntity))
            {
                return false;
            }
        }
        return true;
    }

    public void RelayFailedToEnter(Entity pedEntity, Entity vehicleEntity, VehicleAccessControllerComponent vehicleAccessControllerComponent)
    {
        FailedToEnter?.Invoke(pedEntity, vehicleEntity, vehicleAccessControllerComponent);
    }
}
