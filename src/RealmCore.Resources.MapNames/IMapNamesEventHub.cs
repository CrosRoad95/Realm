namespace RealmCore.Resources.MapNames;

internal interface IMapNamesEventHub
{
    void Add(int id, string name, Color color, float x, float y, float z, ushort dimension = 0, byte interior = 0, Element? attachedTo = null, int category = 0, bool scoped = false);
    void Remove(int id);
    void SetVisibleCategories(int[] categories);
    void SetName(int id, string name);
}
