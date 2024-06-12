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
        string fullPath = Path.Combine(_basePath, path);
        if (Directory.Exists(fullPath))
        {
            return Directory.GetFiles(fullPath)
                .Select(x => Path.GetRelativePath(currentDirectory, x))
                .ToArray();
        }
        return Array.Empty<string>();
    }

    public async Task<string?> ReadAllText(string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (File.Exists(fullPath))
            return await File.ReadAllTextAsync(fullPath);
        return null;
    }
}
