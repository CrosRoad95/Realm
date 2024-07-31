namespace RealmCore.Server.Modules.Players;

public sealed class PlayerUpgradesFeature : IPlayerFeature, IEnumerable<int>, IUsesUserPersistentData
{
    public event Action<PlayerUpgradesFeature, int, bool>? Added;
    public event Action<PlayerUpgradesFeature, int>? Removed;
    public event Action? VersionIncreased;

    private ICollection<UserUpgradeData> _upgrades = [];
    private readonly object _lock = new();

    public RealmPlayer Player { get; init; }
    public PlayerUpgradesFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
        {
            _upgrades = userData.Upgrades;
            foreach (var userUpgradeData in userData.Upgrades)
            {
                Added?.Invoke(this, userUpgradeData.UpgradeId, true);
            }
        }
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

            VersionIncreased?.Invoke();
            Added?.Invoke(this, upgradeId, false);
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
            VersionIncreased?.Invoke();
            Removed?.Invoke(this, upgradeId);
            return true;
        }
    }

    public IEnumerator<int> GetEnumerator()
    {
        int[] view;
        lock (_lock)
            view = [.. _upgrades.Select(x => x.UpgradeId)];

        foreach (var upgradeId in view)
        {
            yield return upgradeId;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
