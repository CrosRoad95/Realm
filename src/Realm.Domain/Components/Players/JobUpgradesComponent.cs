namespace Realm.Domain.Components.Players;

public class JobUpgradesComponent : Component
{
    public struct SingleJobUpgrade
    {
        public short jobId;
        public string name;
    }

    private readonly List<SingleJobUpgrade> _upgrades = new();
    public IEnumerable<SingleJobUpgrade> Upgrades => _upgrades;

    public event Action<Entity, short, string>? JobUpgradeAdded;

    public JobUpgradesComponent()
    {

    }

    public JobUpgradesComponent(ICollection<JobUpgrade> jobUpgrades)
    {
        _upgrades = jobUpgrades.Select(x => new SingleJobUpgrade
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
        _upgrades.Add(new SingleJobUpgrade
        {
            jobId = jobId,
            name = upgradeName,
        });

        JobUpgradeAdded?.Invoke(Entity, jobId, upgradeName);
    }
}
