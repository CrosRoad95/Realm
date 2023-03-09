namespace Realm.Domain.Registries;

public class LevelsRegistry
{
    private readonly Dictionary<uint, uint> _levels = new();
    public LevelsRegistry() { }

    public void AddLevel(LevelRegistryEntry levelRegistryEntry)
    {
        if (_levels.ContainsKey(levelRegistryEntry.Level))
            throw new Exception($"Level {levelRegistryEntry.Level} already exists.");
        if(levelRegistryEntry.Level != 1)
        {
            if (!_levels.ContainsKey(levelRegistryEntry.Level - 1))
            {
                throw new Exception($"Could not add level {levelRegistryEntry.Level} because level {levelRegistryEntry.Level - 1} is not defined.");
            }
        }
        _levels[levelRegistryEntry.Level] = levelRegistryEntry.RequiredExperience;
    }

    public uint GetExperienceRequiredForLevel(uint level)
    {
        if(_levels.ContainsKey(level))
            return _levels[level];
        return uint.MaxValue;
    }
}
