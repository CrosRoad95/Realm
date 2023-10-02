namespace RealmCore.Server.Interfaces;

public interface IMapsService
{
    List<string> Maps { get; }

    void LoadAllMapsFor(Entity entity);
    void LoadMapFor(string name, Entity entity);
    void RegisterMapFromMemory(string name, IEnumerable<WorldObject> worldObjects);
    Map RegisterMapFromMapFile(string name, string fileName);
    MapsWatcherRegistration RegisterMapsPath(string path);
    void LoadMapForAll(Map map);
}
