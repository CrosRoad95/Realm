namespace RealmCore.Resources.Assets.Classes;

internal class FileSystemFont : IAssetFont
{
    public string Name { get; }
    public string Path { get; }
    public string Checksum { get; } = string.Empty;

    public FileSystemFont(string name, string path)
    {
        Name = name;
        Path = path;
    }
}
