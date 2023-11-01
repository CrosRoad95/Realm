namespace RealmCore.Server.Interfaces;

public interface IMapsService
{
    IReadOnlyList<string> LoadedMaps { get; }
    bool IsLoaded(string name);
    bool Load(string name);
    bool Unload(string name);
}
