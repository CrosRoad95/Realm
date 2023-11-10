namespace RealmCore.Server.Services.Players;

internal class PlayerJobUpgradesService : IPlayerJobUpgradesService
{
    private ICollection<JobUpgradeData> _jobUpgrades = [];
    private readonly object _lock = new();

    public event Action<IPlayerJobUpgradesService, JobUpgradeDTO>? Added;
    public event Action<IPlayerJobUpgradesService, JobUpgradeDTO>? Removed;

    public RealmPlayer Player { get; private set; }
    public PlayerJobUpgradesService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService)
    {
        lock (_lock)
            _jobUpgrades = playerUserService.User.JobUpgrades;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService)
    {
        lock (_lock)
            _jobUpgrades = [];
    }


    private JobUpgradeData? InternalGetUpgrade(short jobId, int upgradeId) => _jobUpgrades.FirstOrDefault(x => x.JobId == jobId && x.UpgradeId == upgradeId);
    private bool InternalHasUpgrade(short jobId, int upgradeId) => _jobUpgrades.Any(x => x.JobId == jobId && x.UpgradeId == upgradeId);

    public IEnumerable<JobUpgradeDTO> GetAllByJobId(short jobId)
    {
        lock (_lock)
            return new List<JobUpgradeDTO>(_jobUpgrades.Where(x => x.JobId == jobId).Select(JobUpgradeDTO.Map));
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
            Added?.Invoke(this, JobUpgradeDTO.Map(jobUpgradeData));
            return true;
        }
    }

    public bool TryRemove(short jobId, int upgradeId)
    {
        lock (_lock)
        {
            var upgrade = InternalGetUpgrade(jobId, upgradeId);
            if (upgrade == null)
                return false;
            _jobUpgrades.Remove(upgrade);
            Removed?.Invoke(this, JobUpgradeDTO.Map(upgrade));
            return true;
        }
    }

    public IEnumerator<JobUpgradeDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<JobUpgradeDTO>(_jobUpgrades.Select(JobUpgradeDTO.Map)).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
