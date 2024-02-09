namespace RealmCore.Server.Modules.Serving;

internal class ServerFilesProvider : IServerFilesProvider
{
    private readonly string _basePath;
    public ServerFilesProvider(string basePath)
    {
        _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
    }

    public string[] GetFiles(string path)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        return Directory.GetFiles(Path.Combine(_basePath, path))
            .Select(x => Path.GetRelativePath(currentDirectory, x))
            .ToArray();
    }

    public async Task<string?> ReadAllText(string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (File.Exists(fullPath))
            return await File.ReadAllTextAsync(fullPath);
        return null;
    }
}
