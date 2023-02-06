using JobUpgrade = Realm.Domain.Concepts.JobUpgrade;
using JobUpgradeData = Realm.Persistance.Data.JobUpgrade;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class JobUpgradesComponent : Component
{
    private readonly List<JobUpgrade> _upgrades = new();
    public IEnumerable<JobUpgrade> Upgrades => _upgrades;

    public event Action<Entity, short, int>? JobUpgradeAdded;

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

    public bool HasJobUpgrade(short jobId, int upgradeId) => _upgrades.Any(x => x.jobId == jobId && x.UpgradeId == upgradeId);

    public void AddJobUpgrade(short jobId, int upgradeId)
    {
        if (HasJobUpgrade(jobId, upgradeId))
            throw new UpgradeAlreadyExistsException(jobId, upgradeId);
        _upgrades.Add(new JobUpgrade
        {
            jobId = jobId,
            UpgradeId = upgradeId,
        });

        JobUpgradeAdded?.Invoke(Entity, jobId, upgradeId);
    }
}
