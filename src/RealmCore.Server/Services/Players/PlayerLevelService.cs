namespace RealmCore.Server.Services.Players;

public interface IPlayerLevelService : IPlayerService
{
    uint NextLevelRequiredExperience { get; }
    uint Level { get; set; }
    uint Experience { get; set; }

    event Action<IPlayerLevelService, uint, bool>? LevelChanged;
    event Action<IPlayerLevelService, uint>? ExperienceChanged;

    void GiveExperience(uint amount);
}

internal sealed class PlayerLevelService : IPlayerLevelService
{
    private readonly object _lock = new();
    private readonly LevelsRegistry _levelsRegistry;
    private readonly IPlayerUserService _playerUserService;
    private uint _level;
    private uint _experience;

    public event Action<IPlayerLevelService, uint, bool>? LevelChanged;
    public event Action<IPlayerLevelService, uint>? ExperienceChanged;

    public RealmPlayer Player { get; init; }
    public PlayerLevelService(PlayerContext playerContext, LevelsRegistry levelsRegistry, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _levelsRegistry = levelsRegistry;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
        {
            _level = playerUserService.User.Level;
            _experience = playerUserService.User.Experience;
        }
    }

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {
        lock (_lock)
        {
            _level = 0;
            _experience = 0;
        }
    }

    public uint NextLevelRequiredExperience
    {
        get
        {
            return _levelsRegistry.GetExperienceRequiredForLevel(Level + 1);
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
                        LevelChanged?.Invoke(this, i, true);
                    }
                }
                else
                {
                    for (var i = _level; value < _level; i--)
                    {
                        _level = i;
                        LevelChanged?.Invoke(this, i, false);
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
            _playerUserService.IncreaseVersion();
            LevelChanged?.Invoke(this, _level, true);
            CheckForNextLevel();
        }
    }
}
