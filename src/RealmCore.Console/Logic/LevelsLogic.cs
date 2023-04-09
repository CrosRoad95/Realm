namespace RealmCore.Console.Logic;

internal class LevelsLogic
{
    private readonly LevelsRegistry _levelsRegistry;

    public LevelsLogic(LevelsRegistry levelsRegistry)
    {
        _levelsRegistry = levelsRegistry;
        for (uint i = 1; i < 100; i++)
        {
            _levelsRegistry.Add(1, new LevelRegistryEntry(i * 25));
        }
    }
}
