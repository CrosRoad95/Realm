using Realm.Resources.Assets.Interfaces;

namespace Realm.Resources.Assets.Classes;

internal class Font : IFont
{
    public string Name { get; }
    public string Path { get; }

    public Font(string name, string path)
    {
        Name = name;
        Path = path;
    }
}
