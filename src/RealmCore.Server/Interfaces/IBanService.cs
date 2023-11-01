using RealmCore.Server.DomainObjects;

namespace RealmCore.Server.Interfaces;

public interface IBanService
{
    event Action<BanDTO>? Banned;
    event Action<string, int, int?>? SerialUnbanned;
    event Action<int, int, int?>? UserUnbanned;

    Task<BanDTO> Ban(RealmPlayer player, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default);
    Task<BanDTO> BanPlayer(RealmPlayer player, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default);
    Task<BanDTO> BanUser(RealmPlayer player, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default);
    Task<Bans> GetBans(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default);
    Task<Bans> GetBansBySerial(string serial, int? type = null, CancellationToken cancellationToken = default);
    Task<Bans> GetBansByUserId(int userId, int? type = null, CancellationToken cancellationToken = default);
    Task<Bans> GetBansByUserIdAndSerial(int userId, string serial, int? type = null, CancellationToken cancellationToken = default);
    Task<bool> IsBanned(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default);
    Task RemoveBan(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default);
}
