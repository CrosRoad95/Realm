namespace Realm.Interfaces.Providers;

public interface IServerFilesProvider
{
    string[] GetFiles(string path);
    Task<string> ReadAllText(string path);
}
