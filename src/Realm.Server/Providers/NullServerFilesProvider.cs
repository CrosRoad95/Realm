namespace Realm.Server.Providers;

public class NullServerFilesProvider : IServerFilesProvider
{
    public static NullServerFilesProvider Instance = new();
    public string[] GetFiles(string path) => Array.Empty<string>();

    public Task<string?> ReadAllText(string path) => Task.FromResult<string?>(null);
}
