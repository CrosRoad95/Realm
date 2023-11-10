
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerJobUpgradesService : IPlayerService, IEnumerable<JobUpgradeDTO>
{
    event Action<IPlayerJobUpgradesService, JobUpgradeDTO>? Added;
    event Action<IPlayerJobUpgradesService, JobUpgradeDTO>? Removed;

    IEnumerable<JobUpgradeDTO> GetAllByJobId(short jobId);
    bool Has(short jobId, int upgradeId);
    bool TryAdd(short jobId, int upgradeId);
    bool TryRemove(short jobId, int upgradeId);
}
