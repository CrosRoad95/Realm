namespace RealmCore.Server.Modules.World;

public class MapNotFoundException : Exception
{
    public MapNotFoundException(string mapName) : base($"Map '{mapName}' not found")
    {

    }
}
