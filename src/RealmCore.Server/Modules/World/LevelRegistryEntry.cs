namespace RealmCore.Server.Modules.World;

public class LevelRegistryEntry : RegistryEntryBase<uint>
{
    public uint RequiredExperience { get; }

    public LevelRegistryEntry(uint requiredExperience)
    {
        RequiredExperience = requiredExperience;
    }
}
