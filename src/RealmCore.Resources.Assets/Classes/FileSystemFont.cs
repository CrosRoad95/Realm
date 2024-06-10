namespace RealmCore.Resources.Assets.Classes;

internal class FileSystemFont : IFont
{
    public string Name { get; }
    public string FontPath { get; }
    public string Checksum { get; } = string.Empty;

    public FileSystemFont(string name, string path)
    {
        Name = name;
        FontPath = path;
    }
}
