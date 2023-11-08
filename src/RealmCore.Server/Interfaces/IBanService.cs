namespace RealmCore.Server.Interfaces;

public interface IBanService
{
    event Action<BanDTO>? Banned;
    event Action<string, int, int?>? SerialUnbanned;
    event Action<int, int, int?>? UserUnbanned;

    Task<IReadOnlyList<BanDTO>> GetBans(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BanDTO>> GetBansBySerial(string serial, int? type = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BanDTO>> GetBansByUserId(int userId, int? type = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BanDTO>> GetBansByUserIdAndSerial(int userId, string serial, int? type = null, CancellationToken cancellationToken = default);
    Task<bool> IsBanned(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default);
    Task RemoveBan(RealmPlayer player, int? type = null, CancellationToken cancellationToken = default);
}
