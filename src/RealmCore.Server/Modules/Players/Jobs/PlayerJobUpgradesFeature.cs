using RealmCore.Persistence.Data;

namespace RealmCore.Server.Modules.Players.Jobs;

public sealed class PlayerJobUpgradesFeature : IPlayerFeature, IEnumerable<JobUpgradeDto>, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<JobUpgradeData> _jobUpgrades = [];

    public event Action<PlayerJobUpgradesFeature, JobUpgradeDto, bool>? Added;
    public event Action<PlayerJobUpgradesFeature, JobUpgradeDto>? Removed;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }

    public PlayerJobUpgradesFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
        {
            foreach (var jobUpgradeData in userData.JobUpgrades)
            {
                _jobUpgrades.Add(jobUpgradeData);
                Added?.Invoke(this, JobUpgradeDto.Map(jobUpgradeData), true);
            }
        }
    }

    private JobUpgradeData? InternalGetUpgrade(short jobId, int upgradeId) => _jobUpgrades.FirstOrDefault(x => x.JobId == jobId && x.UpgradeId == upgradeId);

    private bool InternalHasUpgrade(short jobId, int upgradeId) => _jobUpgrades.Any(x => x.JobId == jobId && x.UpgradeId == upgradeId);

    public JobUpgradeDto[] GetAllByJobId(short jobId)
    {
        lock (_lock)
            return _jobUpgrades.Where(x => x.JobId == jobId).Select(JobUpgradeDto.Map).ToArray();
    }

    public bool Has(short jobId, int upgradeId)
    {
        lock (_lock)
            return InternalHasUpgrade(jobId, upgradeId);
    }

    public bool TryAdd(short jobId, int upgradeId)
    {
        JobUpgradeData jobUpgradeData;
        lock (_lock)
        {
            if (InternalHasUpgrade(jobId, upgradeId))
                return false;
            jobUpgradeData = new JobUpgradeData
            {
                JobId = jobId,
                UpgradeId = upgradeId
            };

            _jobUpgrades.Add(jobUpgradeData);
        }

        VersionIncreased?.Invoke();
        Added?.Invoke(this, JobUpgradeDto.Map(jobUpgradeData), false);
        return true;
    }

    public bool TryRemove(short jobId, int upgradeId)
    {
        JobUpgradeData? jobUpgradeData;
        bool removed;
        lock (_lock)
        {
            jobUpgradeData = InternalGetUpgrade(jobId, upgradeId);
            if (jobUpgradeData == null)
                return false;
            removed = _jobUpgrades.Remove(jobUpgradeData);
        }

        VersionIncreased?.Invoke();
        Removed?.Invoke(this, JobUpgradeDto.Map(jobUpgradeData));
        return true;
    }

    public IEnumerator<JobUpgradeDto> GetEnumerator()
    {
        JobUpgradeData[] view;
        {
            lock (_lock)
                view = [.. _jobUpgrades];
        }

        foreach (var jobUpgradeData in view)
        {
            yield return JobUpgradeDto.Map(jobUpgradeData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
