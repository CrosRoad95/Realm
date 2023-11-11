namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerBansService : IPlayerService, IEnumerable<BanDTO>
{
    event Action<IPlayerBansService, BanDTO>? Added;
    event Action<IPlayerBansService, BanDTO>? Removed;

    void Add(int type, DateTime? until = null, string? reason = null, string? responsible = null);
    bool IsBanned(int type);
    bool RemoveById(int banId);
    bool RemoveByType(int type);
    BanDTO? TryGet(int type);
}
