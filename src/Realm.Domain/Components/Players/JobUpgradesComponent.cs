using JobUpgrade = Realm.Domain.Concepts.JobUpgrade;
using JobUpgradeData = Realm.Persistance.Data.JobUpgrade;

namespace Realm.Domain.Components.Players;

public class JobUpgradesComponent : Component
{
    private readonly List<JobUpgrade> _upgrades = new();
    public IEnumerable<JobUpgrade> Upgrades => _upgrades;

    public event Action<Entity, short, string>? JobUpgradeAdded;

    public JobUpgradesComponent()
    {

    }

    public JobUpgradesComponent(ICollection<JobUpgradeData> jobUpgrades)
    {
        _upgrades = jobUpgrades.Select(x => new JobUpgrade
        {
            jobId = x.JobId,
            name = x.Name,
        }).ToList();
    }

    public bool HasJobUpgrade(short jobId, string upgradeName) => _upgrades.Any(x => x.jobId == jobId && x.name == upgradeName);

    public void AddJobUpgrade(short jobId, string upgradeName)
    {
        if (HasJobUpgrade(jobId, upgradeName))
            throw new UpgradeAlreadyExistsException(jobId, upgradeName);
        _upgrades.Add(new JobUpgrade
        {
            jobId = jobId,
            name = upgradeName,
        });

        JobUpgradeAdded?.Invoke(Entity, jobId, upgradeName);
    }
}
