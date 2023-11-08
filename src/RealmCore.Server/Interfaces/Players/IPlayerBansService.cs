namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerBansService : IPlayerService
{
    IReadOnlyList<BanDTO> AllActive { get; }

    event Action<BanDTO>? Added;
    event Action<BanDTO>? Removed;

    void Add(int type, DateTime? until = null, string? reason = null, string? responsible = null);
    bool IsBanned(int type);
    bool RemoveById(int banId);
    bool RemoveByType(int type);
    BanDTO? TryGetBan(int type);
}
