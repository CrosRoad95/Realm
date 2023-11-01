namespace RealmCore.Server.Exceptions;

public class MapNotFoundException : Exception
{
    public MapNotFoundException(string mapName) : base($"Map '{mapName}' not found")
    {

    }
}
