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
    uint Level { get; set; }
    uint Experience { get; set; }

    event Action<IPlayerLevelFeature, uint, LevelChange>? LevelChanged;
    event Action<IPlayerLevelFeature, uint>? ExperienceChanged;

    void GiveExperience(uint amount);
}

internal sealed class PlayerLevelFeature : IPlayerLevelFeature
{
    private readonly object _lock = new();
    private readonly LevelsCollection _levelsCollection;
    private readonly IPlayerUserFeature _playerUserFeature;
    private uint _level;
    private uint _experience;

    public event Action<IPlayerLevelFeature, uint, LevelChange>? LevelChanged;
    public event Action<IPlayerLevelFeature, uint>? ExperienceChanged;

    public RealmPlayer Player { get; init; }
    public PlayerLevelFeature(PlayerContext playerContext, LevelsCollection levelsCollection, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _levelsCollection = levelsCollection;
        _playerUserFeature = playerUserFeature;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
        {
            _level = playerUserFeature.User.Level;
            _experience = playerUserFeature.User.Experience;
            LevelChanged?.Invoke(this, _level, LevelChange.Set);
            ExperienceChanged?.Invoke(this, _experience);
        }
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
        {
            _level = 0;
            _experience = 0;
            LevelChanged?.Invoke(this, _level, LevelChange.Set);
            ExperienceChanged?.Invoke(this, _experience);
        }
    }

    public uint NextLevelRequiredExperience
    {
        get
        {
            return _levelsCollection.GetExperienceRequiredForLevel(Level + 1);
        }
    }

    public uint Level
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
            _playerUserFeature.IncreaseVersion();
            LevelChanged?.Invoke(this, _level, LevelChange.Increase);
            CheckForNextLevel();
        }
    }
}
