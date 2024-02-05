namespace RealmCore.Server.Services.Players;

public interface IPlayerUpgradeService : IPlayerService
{

    event Action<IPlayerUpgradeService, int>? Added;
    event Action<IPlayerUpgradeService, int>? Removed;

    bool Has(int upgradeId);
    bool TryAdd(int upgradeId);
    bool TryRemove(int upgradeId);
}

internal class PlayerUpgradeService : IPlayerUpgradeService
{
    public event Action<IPlayerUpgradeService, int>? Added;
    public event Action<IPlayerUpgradeService, int>? Removed;

    private ICollection<UserUpgradeData> _upgrades = [];
    private readonly object _lock = new();
    private readonly IPlayerUserService _playerUserService;

    public RealmPlayer Player { get; }
    public PlayerUpgradeService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _upgrades = playerUserService.User.Upgrades;
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
            _upgrades = [];
    }

    internal UserUpgradeData? GetUpgrade(int upgradeId) => _upgrades.FirstOrDefault(x =>  x.UpgradeId == upgradeId);
    internal bool InternalHasUpgrade(int upgradeId) => _upgrades.Any(x =>  x.UpgradeId == upgradeId);

    public bool Has(int upgradeId)
    {
        lock (_lock)
            return InternalHasUpgrade(upgradeId);
    }

    public bool TryAdd(int upgradeId)
    {
        lock (_lock)
        {
            if (InternalHasUpgrade(upgradeId))
                return false;
            _upgrades.Add(new UserUpgradeData
            {
                UpgradeId = upgradeId,
            });
            _playerUserService.IncreaseVersion();
            Added?.Invoke(this, upgradeId);
            return true;
        }
    }

    public bool TryRemove(int upgradeId)
    {
        lock (_lock)
        {
            var upgrade = GetUpgrade(upgradeId);
            if (upgrade == null)
                return false;
            _upgrades.Remove(upgrade);
            _playerUserService.IncreaseVersion();
            Removed?.Invoke(this, upgradeId);
            return true;
        }
    }
}
