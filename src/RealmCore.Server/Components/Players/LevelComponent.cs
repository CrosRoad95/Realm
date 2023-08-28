using RealmCore.ECS.Components;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class LevelComponent : Component
{
    public uint NextLevelRequiredExperience
    {
        get
        {
            ThrowIfDisposed();
            return _levelsRegistry.GetExperienceRequiredForLevel(Level + 1);
        }
    }

    private uint _level;
    private uint _experience;

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

    private readonly object _lock = new();
    private readonly LevelsRegistry _levelsRegistry;

    public event Action<LevelComponent, uint, bool>? LevelChanged;
    public event Action<LevelComponent, uint>? ExperienceChanged;

    public LevelComponent(LevelsRegistry levelsRegistry)
    {
        _levelsRegistry = levelsRegistry;
    }

    public LevelComponent(uint level, uint experience)
    {
        _level = level;
        _experience = experience;
    }

    public void GiveExperience(uint amount)
    {
        ThrowIfDisposed();

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
