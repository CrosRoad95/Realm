namespace Realm.Domain.Registries;

public class LevelRegistryEntry
{
    public uint Level { get; }
    public uint RequiredExperience { get; }

    public LevelRegistryEntry(uint level, uint requiredExperience)
    {
        Level = level;
        RequiredExperience = requiredExperience;
    }
}
