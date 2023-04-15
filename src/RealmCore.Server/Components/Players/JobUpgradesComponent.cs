namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class JobUpgradesComponent : Component
{
    private readonly List<JobUpgrade> _upgrades = new();
    private readonly object _upgradesLock = new();
    public IReadOnlyList<JobUpgrade> Upgrades
    {
        get
        {
            lock (_upgradesLock)
                return new List<JobUpgrade>(_upgrades);
        }
    }

    public event Action<JobUpgradesComponent, JobUpgrade>? UpgradeAdded;
    public event Action<JobUpgradesComponent, JobUpgrade>? UpgradeRemoved;
    public JobUpgradesComponent()
    {

    }

    internal JobUpgradesComponent(ICollection<JobUpgradeData> jobUpgrades)
    {
        _upgrades = jobUpgrades.Select(x => new JobUpgrade
        {
            jobId = x.JobId,
            upgradeId = x.UpgradeId,
        }).ToList();
    }

    private bool InternalHasJobUpgrade(short jobId, int upgradeId) => _upgrades.Any(x => x.jobId == jobId && x.upgradeId == upgradeId);

    public bool HasJobUpgrade(short jobId, int upgradeId)
    {
        ThrowIfDisposed();

        lock (_upgradesLock)
            return InternalHasJobUpgrade(jobId, upgradeId);
    }

    public bool TryAddJobUpgrade(short jobId, int upgradeId)
    {
        ThrowIfDisposed();

        lock (_upgradesLock)
        {
            if (InternalHasJobUpgrade(jobId, upgradeId))
                return false;

            var upgrade = new JobUpgrade
            {
                jobId = jobId,
                upgradeId = upgradeId,
            };

            _upgrades.Add(upgrade);
            UpgradeAdded?.Invoke(this, upgrade);
            return true;
        }
    }

    public bool TryRemoveJobUpgrade(short jobId, int upgradeId, bool all = false)
    {
        ThrowIfDisposed();

        lock (_upgradesLock)
        {
            if (!InternalHasJobUpgrade(jobId, upgradeId))
                return false;


            if (all)
            {
                var upgradesToRemove = _upgrades.Where(x => x.jobId == jobId && x.upgradeId == upgradeId);
                _upgrades.RemoveAll(x => x.jobId == jobId && x.upgradeId == upgradeId);
                foreach (var upgradeToRemove in upgradesToRemove)
                    UpgradeRemoved?.Invoke(this, upgradeToRemove);
            }
            else
            {
                var upgradeToRemove = _upgrades.First(x => x.jobId == jobId && x.upgradeId == upgradeId);
                _upgrades.Remove(upgradeToRemove);
                UpgradeRemoved?.Invoke(this, upgradeToRemove);
            }
            return true;
        }
    }
}
