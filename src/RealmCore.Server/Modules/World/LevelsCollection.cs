namespace RealmCore.Server.Modules.World;

public class LevelsCollectionItem : CollectionItemBase<uint>
{
    public uint RequiredExperience { get; }

    public LevelsCollectionItem(uint requiredExperience)
    {
        RequiredExperience = requiredExperience;
    }
}

public class LevelsCollection : CollectionBase<uint, LevelsCollectionItem>
{
    public uint GetExperienceRequiredForLevel(uint level)
    {
        if (_entries.ContainsKey(level))
            return _entries[level].RequiredExperience;
        return uint.MaxValue;
    }
}
