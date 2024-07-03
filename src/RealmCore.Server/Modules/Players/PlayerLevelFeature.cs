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

    event Action<IPlayerLevelFeature, uint, LevelChange>? Changed;
    event Action<IPlayerLevelFeature, uint, uint>? ExperienceChanged;

    void GiveExperience(uint amount);
}

internal sealed class PlayerLevelFeature : IPlayerLevelFeature, IUsesUserPersistentData
{
    private readonly ReaderWriterLockSlimScoped _writerLockSlim = new();
    private readonly LevelsCollection? _levelsCollection;
    private uint _level;
    private uint _experience;
    private UserData? _userData;

    public event Action<IPlayerLevelFeature, uint, LevelChange>? Changed;
    public event Action<IPlayerLevelFeature, uint, uint>? ExperienceChanged;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerLevelFeature(PlayerContext playerContext, LevelsCollection? levelsCollection = null)
    {
        Player = playerContext.Player;
        _levelsCollection = levelsCollection;
    }

    public void LogIn(UserData userData)
    {
        using var _ = _writerLockSlim.BeginWrite();

        var before = _experience;
        _level = userData.Level;
        _experience = userData.Experience;
        _userData = userData;
        Changed?.Invoke(this, _level, LevelChange.Set);
        ExperienceChanged?.Invoke(this, before, _experience);
    }

    public uint NextLevelRequiredExperience
    {
        get
        {
            if (_levelsCollection == null)
                throw new NotSupportedException();

            return _levelsCollection.GetExperienceRequiredForLevel(Current + 1);
        }
    }

    public uint Current
    {
        get => _level; set
        {
            using var _ = _writerLockSlim.BeginWrite();

            if (value > _level)
            {
                for (var i = _level; value > _level; i++)
                {
                    _level = i;
                    Changed?.Invoke(this, i, LevelChange.Increase);
                }
            }
            else
            {
                for (var i = _level; value < _level; i--)
                {
                    _level = i;
                    Changed?.Invoke(this, i, LevelChange.Decrease);
                }
            }
            _level = value;
            if(_userData != null)
            {
                _userData.Level = _level;
            }
        }
    }

    public uint Experience
    {
        get => _experience; set
        {
            using var _ = _writerLockSlim.BeginWrite();

            if (_experience != value)
            {
                var before = _experience;
                _experience = value;
                CheckForNextLevel();
                if (_userData != null)
                    _userData.Experience = _experience;
                ExperienceChanged?.Invoke(this, before, value);
            }
        }
    }

    public void GiveExperience(uint amount)
    {
        if (amount == 0)
            return;

        using var _ = _writerLockSlim.BeginWrite();

        var before = _experience;
        _experience += amount;
        if (_userData != null)
            _userData.Experience = _experience;

        CheckForNextLevel();
        ExperienceChanged?.Invoke(this, before, _experience);
        VersionIncreased?.Invoke();
    }

    private void CheckForNextLevel()
    {
        if (_experience >= NextLevelRequiredExperience)
        {
            _experience -= NextLevelRequiredExperience;
            _level++;
            if (_userData != null)
                _userData.Level = _level;

            Changed?.Invoke(this, _level, LevelChange.Increase);
            CheckForNextLevel();
        }
    }
}
