namespace RealmCore.Server.Modules.Players;

public interface IPlayerUpgradeFeature : IPlayerFeature
{

    event Action<IPlayerUpgradeFeature, int>? Added;
    event Action<IPlayerUpgradeFeature, int>? Removed;

    bool Has(int upgradeId);
    bool TryAdd(int upgradeId);
    bool TryRemove(int upgradeId);
}

internal sealed class PlayerUpgradeFeature : IPlayerUpgradeFeature
{
    public event Action<IPlayerUpgradeFeature, int>? Added;
    public event Action<IPlayerUpgradeFeature, int>? Removed;

    private ICollection<UserUpgradeData> _upgrades = [];
    private readonly object _lock = new();
    private readonly IPlayerUserFeature _playerUserFeature;

    public RealmPlayer Player { get; init; }
    public PlayerUpgradeFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _upgrades = playerUserFeature.User.Upgrades;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _upgrades = [];
    }

    internal UserUpgradeData? GetUpgrade(int upgradeId) => _upgrades.FirstOrDefault(x => x.UpgradeId == upgradeId);
    internal bool InternalHasUpgrade(int upgradeId) => _upgrades.Any(x => x.UpgradeId == upgradeId);

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
            _playerUserFeature.IncreaseVersion();
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
            _playerUserFeature.IncreaseVersion();
            Removed?.Invoke(this, upgradeId);
            return true;
        }
    }
}
