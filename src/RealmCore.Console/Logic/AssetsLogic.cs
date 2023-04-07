using RealmCore.Resources.Assets;

namespace RealmCore.Console.Logic;

internal sealed class AssetsLogic
{
    private const string _basePath = "../../../Server/Assets";

    public AssetsLogic(AssetsRegistry assetsRegistry)
    {
        foreach (var item in Directory.GetFiles(_basePath, "*.*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetRelativePath(_basePath, item);

            switch (Path.GetDirectoryName(fileName))
            {
                case "Fonts":
                    assetsRegistry.AddFont(Path.GetFileName(fileName), $"Server/Assets/{fileName}");
                    break;
            }
        }
    }
}
