using RealmCore.Server.Dto;

namespace RealmCore.Server.Services.Players;

public interface IPlayerJobUpgradesService : IPlayerService, IEnumerable<JobUpgradeDto>
{
    event Action<IPlayerJobUpgradesService, JobUpgradeDto>? Added;
    event Action<IPlayerJobUpgradesService, JobUpgradeDto>? Removed;

    IEnumerable<JobUpgradeDto> GetAllByJobId(short jobId);
    bool Has(short jobId, int upgradeId);
    bool TryAdd(short jobId, int upgradeId);
    bool TryRemove(short jobId, int upgradeId);
}

internal sealed class PlayerJobUpgradesService : IPlayerJobUpgradesService
{
    private readonly object _lock = new();
    private ICollection<JobUpgradeData> _jobUpgrades = [];
    private readonly IPlayerUserService _playerUserService;

    public event Action<IPlayerJobUpgradesService, JobUpgradeDto>? Added;
    public event Action<IPlayerJobUpgradesService, JobUpgradeDto>? Removed;

    public RealmPlayer Player { get; init; }
    public PlayerJobUpgradesService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _jobUpgrades = playerUserService.User.JobUpgrades;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
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
            _playerUserService.IncreaseVersion();
            Added?.Invoke(this, JobUpgradeDto.Map(jobUpgradeData));
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
            _playerUserService.IncreaseVersion();
            Removed?.Invoke(this, JobUpgradeDto.Map(upgrade));
            return true;
        }
    }

    public IEnumerator<JobUpgradeDto> GetEnumerator()
    {
        lock (_lock)
            return new List<JobUpgradeDto>(_jobUpgrades.Select(JobUpgradeDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
