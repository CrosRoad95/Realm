namespace RealmCore.Server.Interfaces.Players;

public interface IPlayersUpgradeService : IPlayerService
{

    event Action<IPlayersUpgradeService, int>? Added;
    event Action<IPlayersUpgradeService, int>? Removed;

    bool Has(int upgradeId);
    bool TryAdd(int upgradeId);
    bool TryRemove(int upgradeId);
}
