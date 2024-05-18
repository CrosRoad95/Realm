namespace RealmCore.Sample.Logic;

internal sealed class AssetsManager : BackgroundService
{
    private const string _basePath = "../../../Server/Assets";
    private readonly AssetsCollection _assetsCollection;

    public AssetsManager(AssetsCollection assetsCollection)
    {
        _assetsCollection = assetsCollection;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Directory.Exists(_basePath))
        {
            foreach (var item in Directory.GetFiles(_basePath, "*.*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetRelativePath(_basePath, item);

                switch (Path.GetDirectoryName(fileName))
                {
                    case "Fonts":
                        _assetsCollection.AddFont(Path.GetFileName(fileName), $"Server/Assets/{fileName}");
                        break;
                }
            }
        }

        return Task.CompletedTask;
    }
}
