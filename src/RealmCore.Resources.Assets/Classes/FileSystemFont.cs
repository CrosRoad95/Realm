namespace RealmCore.Resources.Assets.Classes;

internal class FileSystemFont : IFont
{
    public string Name { get; }
    public string FontPath { get; }

    public FileSystemFont(string name, string path)
    {
        Name = name;
        FontPath = path;
    }
}
