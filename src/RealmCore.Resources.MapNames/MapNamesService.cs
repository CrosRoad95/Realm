namespace RealmCore.Resources.MapNames;

public interface IMapNamesService
{
    internal event Action<MapNameId, MapName>? Added;
    internal event Action<MapNameId, MapName, Player[]>? AddedFor;
    internal event Action<MapNameId>? Removed;
    internal event Action<MapNameId, Player[]>? RemovedFor;
    internal event Action<Player, int[]>? CategoryVisibilityChanged;
    internal event Action<MapNameId, string>? NameChanged;
    internal event Action<MapNameId, string, Player[]>? NameChangedFor;

    MapNameId AddName(MapName mapName);
    MapNameId AddNameFor(MapName mapName, params Player[] players);
    void Remove(MapNameId nameId);
    void RemoveFor(MapNameId nameId, params Player[] players);
    void SetName(MapNameId nameId, string name);
    void SetNameFor(MapNameId nameId, string name, params Player[] players);
    void SetVisibleCategories(Player player, int[] categories);
}

internal sealed class MapNamesService : IMapNamesService
{
    private int _id;

    public event Action<MapNameId, MapName>? Added;
    public event Action<MapNameId, MapName, Player[]>? AddedFor;
    public event Action<MapNameId>? Removed;
    public event Action<MapNameId, Player[]>? RemovedFor;
    public event Action<Player, int[]>? CategoryVisibilityChanged;
    public event Action<MapNameId, string>? NameChanged;
    public event Action<MapNameId, string, Player[]>? NameChangedFor;

    private MapNameId GetId() => new(Interlocked.Increment(ref _id));

    public MapNameId AddName(MapName mapName)
    {
        var id = GetId();
        Added?.Invoke(id, mapName);
        return id;
    }

    public MapNameId AddNameFor(MapName mapName, params Player[] players)
    {
        if(players.Length == 0)
            throw new InvalidOperationException("Sequence contains no players");

        var id = GetId();
        AddedFor?.Invoke(id, mapName, players);
        return id;
    }

    public void Remove(MapNameId nameId)
    {
        Removed?.Invoke(nameId);
    }

    public void RemoveFor(MapNameId nameId, params Player[] players)
    {
        if (players.Length == 0)
            throw new InvalidOperationException("Sequence contains no players");

        RemovedFor?.Invoke(nameId, players);
    }

    public void SetVisibleCategories(Player player, int[] categories)
    {
        CategoryVisibilityChanged?.Invoke(player, categories);
    }

    public void SetName(MapNameId nameId, string name)
    {
        NameChanged?.Invoke(nameId, name);
    }

    public void SetNameFor(MapNameId nameId, string name, params Player[] players)
    {
        if (players.Length == 0)
            throw new InvalidOperationException("Sequence contains no players");

        NameChangedFor?.Invoke(nameId, name, players);
    }
}

public struct MapNameId
{
    internal int _id;

    public MapNameId(int id)
    {
        _id = id;
    }
}