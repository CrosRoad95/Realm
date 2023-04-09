namespace RealmCore.Server.Registries;

public class LevelsRegistry : RegistryBase<uint, LevelRegistryEntry>
{
    public uint GetExperienceRequiredForLevel(uint level)
    {
        if (_entries.ContainsKey(level))
            return _entries[level].RequiredExperience;
        return uint.MaxValue;
    }
}
