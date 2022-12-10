namespace Realm.Server.Providers;

internal class ServerFilesProvider : IServerFilesProvider
{
    private readonly string _basePath;
    public ServerFilesProvider(string basePath)
    {
        _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
    }

    public string[] GetFiles(string path)
    {
        return Directory.GetFiles(Path.Combine(_basePath, path));
    }

    public async Task<string> ReadAllText(string path)
    {
        return await File.ReadAllTextAsync(Path.Combine(_basePath, path));
    }
}
