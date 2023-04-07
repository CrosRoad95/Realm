using RealmCore.Resources.Assets.Interfaces;

namespace RealmCore.Resources.Assets.Classes;

internal class Font : IFont
{
    public string Name { get; }
    public string FontPath { get; }

    public Font(string name, string path)
    {
        Name = name;
        FontPath = path;
    }
}
