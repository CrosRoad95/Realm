namespace RealmCore.Sample.Logic;

internal class LevelsLogic
{
    public LevelsLogic(LevelsCollection levelsCollection)
    {
        for (uint i = 1; i < 100; i++)
        {
            levelsCollection.Add(i, new LevelsCollectionItem(i * 25));
        }
    }
}
