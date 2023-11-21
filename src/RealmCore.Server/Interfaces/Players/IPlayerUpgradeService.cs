namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerUpgradeService : IPlayerService
{

    event Action<IPlayerUpgradeService, int>? Added;
    event Action<IPlayerUpgradeService, int>? Removed;

    bool Has(int upgradeId);
    bool TryAdd(int upgradeId);
    bool TryRemove(int upgradeId);
}
