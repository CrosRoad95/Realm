namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class LevelComponent : Component
{
    [Inject]
    private LevelsRegistry LevelsRegistry { get; set; } = default!;

    public uint NextLevelRequiredExperience => LevelsRegistry.GetExperienceRequiredForLevel(Level + 1);
    public uint Level { get; private set; }
    public uint Experience { get; private set; }

    private object _lock = new();

    public event Action<LevelComponent, uint>? LevelChanged;

    public LevelComponent()
    {

    }

    public LevelComponent(uint level, uint experience)
    {
        Level = level;
        Experience = experience;
    }

    public void GiveExperience(uint amount)
    {
        lock(_lock)
        {
            Experience += amount;
            CheckForNextLevel();
        }
    }

    private void CheckForNextLevel()
    {
        if(Experience >= NextLevelRequiredExperience)
        {
            Experience -= NextLevelRequiredExperience;
            Level++;
            LevelChanged?.Invoke(this, Level);
            CheckForNextLevel();
        }
    }
}
