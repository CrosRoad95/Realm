namespace RealmCore.Server.Modules.Players;

public enum LevelChange
{
    Set,
    Increase,
    Decrease
}

public interface IPlayerLevelFeature : IPlayerFeature
{
    uint NextLevelRequiredExperience { get; }
    uint Current { get; set; }
    uint Experience { get; set; }

    event Action<IPlayerLevelFeature, uint, LevelChange>? LevelChanged;
    event Action<IPlayerLevelFeature, uint>? ExperienceChanged;

    void GiveExperience(uint amount);
}

internal sealed class PlayerLevelFeature : IPlayerLevelFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private readonly LevelsCollection _levelsCollection;
    private uint _level;
    private uint _experience;

    public event Action<IPlayerLevelFeature, uint, LevelChange>? LevelChanged;
    public event Action<IPlayerLevelFeature, uint>? ExperienceChanged;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerLevelFeature(PlayerContext playerContext, LevelsCollection levelsCollection)
    {
        Player = playerContext.Player;
        _levelsCollection = levelsCollection;
    }

    public void SignIn(UserData userData)
    {
        _level = userData.Level;
        _experience = userData.Experience;
        LevelChanged?.Invoke(this, _level, LevelChange.Set);
        ExperienceChanged?.Invoke(this, _experience);
    }

    public void SignOut()
    {
        _level = 0;
        _experience = 0;
        LevelChanged?.Invoke(this, _level, LevelChange.Set);
        ExperienceChanged?.Invoke(this, _experience);
    }

    public uint NextLevelRequiredExperience
    {
        get
        {
            return _levelsCollection.GetExperienceRequiredForLevel(Current + 1);
        }
    }

    public uint Current
    {
        get => _level; set
        {
            lock (_lock)
            {
                if (value > _level)
                {
                    for (var i = _level; value > _level; i++)
                    {
                        _level = i;
                        LevelChanged?.Invoke(this, i, LevelChange.Increase);
                    }
                }
                else
                {
                    for (var i = _level; value < _level; i--)
                    {
                        _level = i;
                        LevelChanged?.Invoke(this, i, LevelChange.Decrease);
                    }
                }
                _level = value;
            }
        }
    }

    public uint Experience
    {
        get => _experience; set
        {
            lock (_lock)
            {
                if (_experience != value)
                {
                    _experience = value;
                    CheckForNextLevel();
                    ExperienceChanged?.Invoke(this, value);
                }
            }
        }
    }

    public void GiveExperience(uint amount)
    {
        lock (_lock)
        {
            _experience += amount;
            CheckForNextLevel();
            ExperienceChanged?.Invoke(this, Experience);
        }
    }

    private void CheckForNextLevel()
    {
        if (_experience >= NextLevelRequiredExperience)
        {
            _experience -= NextLevelRequiredExperience;
            _level++;
            VersionIncreased?.Invoke();
            LevelChanged?.Invoke(this, _level, LevelChange.Increase);
            CheckForNextLevel();
        }
    }
}
