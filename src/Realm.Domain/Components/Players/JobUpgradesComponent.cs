using JobUpgrade = Realm.Domain.Concepts.JobUpgrade;
using JobUpgradeData = Realm.Persistance.Data.JobUpgrade;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class JobUpgradesComponent : Component
{
    private readonly List<JobUpgrade> _upgrades = new();
    private readonly object _upgradesLock = new();
    public IReadOnlyList<JobUpgrade> Upgrades => _upgrades;

    public JobUpgradesComponent()
    {

    }

    internal JobUpgradesComponent(ICollection<JobUpgradeData> jobUpgrades)
    {
        _upgrades = jobUpgrades.Select(x => new JobUpgrade
        {
            jobId = x.JobId,
            UpgradeId = x.UpgradeId,
        }).ToList();
    }

    private bool InternalHasJobUpgrade(short jobId, int upgradeId) => _upgrades.Any(x => x.jobId == jobId && x.UpgradeId == upgradeId);

    public bool HasJobUpgrade(short jobId, int upgradeId)
    {
        lock (_upgradesLock)
            return InternalHasJobUpgrade(jobId, upgradeId);
    }

    public bool TryAddJobUpgrade(short jobId, int upgradeId)
    {
        lock (_upgradesLock)
        {
            if (InternalHasJobUpgrade(jobId, upgradeId))
                return false;

            _upgrades.Add(new JobUpgrade
            {
                jobId = jobId,
                UpgradeId = upgradeId,
            });
            return true;
        }
    }
}
