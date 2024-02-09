namespace RealmCore.Sample.Logic;

internal sealed class AssetsLogic
{
    private const string _basePath = "../../../Server/Assets";

    public AssetsLogic(AssetsCollection assetsCollection)
    {
        if (Directory.Exists(_basePath))
        {
            foreach (var item in Directory.GetFiles(_basePath, "*.*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetRelativePath(_basePath, item);

                switch (Path.GetDirectoryName(fileName))
                {
                    case "Fonts":
                        assetsCollection.AddFont(Path.GetFileName(fileName), $"Server/Assets/{fileName}");
                        break;
                }
            }
        }
    }
}
