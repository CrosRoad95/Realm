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

    public async Task<string?> ReadAllText(string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (File.Exists(fullPath))
            return await File.ReadAllTextAsync(fullPath);
        return null;
    }
}
