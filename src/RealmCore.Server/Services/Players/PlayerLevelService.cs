namespace RealmCore.Server.Services.Players;

internal class PlayerLevelService : IPlayerLevelService
{
    private readonly object _lock = new();
    private readonly LevelsRegistry _levelsRegistry;
    private uint _level;
    private uint _experience;

    public RealmPlayer Player { get; private set; }
    public PlayerLevelService(PlayerContext playerContext, LevelsRegistry levelsRegistry, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _levelsRegistry = levelsRegistry;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService)
    {
        lock (_lock)
        {
            _level = playerUserService.User.Level;
            _experience = playerUserService.User.Experience;
        }
    }

    private void HandleSignedOut(IPlayerUserService playerUserService)
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

    public uint Experience
    {
        get => _experience; set
        {
            if (_experience != value)
            {
                _experience = value;
                CheckForNextLevel();
                ExperienceChanged?.Invoke(this, value);
            }
        }
    }

    public event Action<IPlayerLevelService, uint, bool>? LevelChanged;
    public event Action<IPlayerLevelService, uint>? ExperienceChanged;

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
            LevelChanged?.Invoke(this, _level, true);
            CheckForNextLevel();
        }
    }
}
