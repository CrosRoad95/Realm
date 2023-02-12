using Realm.Resources.Assets;
using Realm.Resources.Assets.Interfaces;

namespace Realm.Console.Services;

public class ServerAssetsService : IServerAssetsProvider
{
    private const string _basePath = "../../../Server/Assets";
    private readonly AssetsRegistry _assetsRegistry;

    public ServerAssetsService(AssetsRegistry assetsRegistry)
    {
        _assetsRegistry = assetsRegistry;
    }

    public IEnumerable<(string, byte[])> Provide()
    {
        foreach (var item in Directory.GetFiles(_basePath, "*.*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetRelativePath(_basePath, item);
            var content = File.ReadAllBytes(item);
            yield return (fileName, content);
            var folder = Path.GetDirectoryName(fileName);
            switch(Path.GetDirectoryName(fileName))
            {
                case "Fonts":
                    _assetsRegistry.AddFont(Path.GetFileName(fileName), fileName);
                    break;
            }
        }
    }
}
