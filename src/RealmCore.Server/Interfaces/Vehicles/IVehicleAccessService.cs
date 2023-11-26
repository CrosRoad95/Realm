

namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehicleAccessService : IVehicleService, IEnumerable<VehicleUserAccessDTO>
{
    IReadOnlyList<VehicleUserAccessDTO> Owners { get; }

    VehicleUserAccessDTO AddAccess(int userId, byte accessType, string? customValue = null);
    VehicleUserAccessDTO AddAccess(RealmPlayer player, byte accessType, string? customValue = null);
    VehicleUserAccessDTO AddAsOwner(RealmPlayer player, string? customValue = null);
    VehicleUserAccessDTO AddAsOwner(int userId, string? customValue = null);
    bool HasAccess(RealmPlayer player);
    bool HasAccess(int userId);
    bool IsOwner(int userId);
    bool IsOwner(RealmPlayer player);
    bool TryGetAccess(RealmPlayer player, out VehicleUserAccessDTO vehicleAccess);
}
