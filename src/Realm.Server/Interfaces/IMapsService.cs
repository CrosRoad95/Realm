namespace Realm.Server.Interfaces;

public interface IMapsService
{
    List<string> Maps { get; }

    void LoadMapFor(string name, Entity entity);
    void RegisterMapFromMemory(string name, IEnumerable<WorldObject> worldObjects);
    void RegisterMapFromXml(string name, string fileName);
}
