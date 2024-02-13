namespace RealmCore.Server.Modules.Players.Jobs;

public interface IPlayerJobUpgradesFeature : IPlayerFeature, IEnumerable<JobUpgradeDto>
{
    event Action<IPlayerJobUpgradesFeature, JobUpgradeDto, bool>? Added;
    event Action<IPlayerJobUpgradesFeature, JobUpgradeDto>? Removed;

    IEnumerable<JobUpgradeDto> GetAllByJobId(short jobId);
    bool Has(short jobId, int upgradeId);
    bool TryAdd(short jobId, int upgradeId);
    bool TryRemove(short jobId, int upgradeId);
}

internal sealed class PlayerJobUpgradesFeature : IPlayerJobUpgradesFeature, IDisposable
{
    private readonly object _lock = new();
    private ICollection<JobUpgradeData> _jobUpgrades = [];
    private readonly IPlayerUserFeature _playerUserFeature;

    public event Action<IPlayerJobUpgradesFeature, JobUpgradeDto, bool>? Added;
    public event Action<IPlayerJobUpgradesFeature, JobUpgradeDto>? Removed;

    public RealmPlayer Player { get; init; }
    public PlayerJobUpgradesFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
        {
            foreach (var jobUpgradeData in playerUserFeature.User.JobUpgrades)
            {
                _jobUpgrades.Add(jobUpgradeData);
                Added?.Invoke(this, JobUpgradeDto.Map(jobUpgradeData), true);
            }
        }
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _jobUpgrades = [];
    }

    private JobUpgradeData? InternalGetUpgrade(short jobId, int upgradeId) => _jobUpgrades.FirstOrDefault(x => x.JobId == jobId && x.UpgradeId == upgradeId);

    private bool InternalHasUpgrade(short jobId, int upgradeId) => _jobUpgrades.Any(x => x.JobId == jobId && x.UpgradeId == upgradeId);

    public IEnumerable<JobUpgradeDto> GetAllByJobId(short jobId)
    {
        lock (_lock)
            return new List<JobUpgradeDto>(_jobUpgrades.Where(x => x.JobId == jobId).Select(JobUpgradeDto.Map));
    }

    public bool Has(short jobId, int upgradeId)
    {
        lock (_lock)
            return InternalHasUpgrade(jobId, upgradeId);
    }

    public bool TryAdd(short jobId, int upgradeId)
    {
        lock (_lock)
        {
            if (InternalHasUpgrade(jobId, upgradeId))
                return false;
            var jobUpgradeData = new JobUpgradeData
            {
                JobId = jobId,
                UpgradeId = upgradeId
            };

            _jobUpgrades.Add(jobUpgradeData);
            _playerUserFeature.IncreaseVersion();
            Added?.Invoke(this, JobUpgradeDto.Map(jobUpgradeData), false);
            return true;
        }
    }

    public bool TryRemove(short jobId, int upgradeId)
    {
        lock (_lock)
        {
            var jobUpgradeData = InternalGetUpgrade(jobId, upgradeId);
            if (jobUpgradeData == null)
                return false;
            _jobUpgrades.Remove(jobUpgradeData);
            _playerUserFeature.IncreaseVersion();
            Removed?.Invoke(this, JobUpgradeDto.Map(jobUpgradeData));
            return true;
        }
    }

    public IEnumerator<JobUpgradeDto> GetEnumerator()
    {
        lock (_lock)
            return new List<JobUpgradeDto>(_jobUpgrades.Select(JobUpgradeDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        _playerUserFeature.SignedIn -= HandleSignedIn;
        _playerUserFeature.SignedOut -= HandleSignedOut;
    }
}
