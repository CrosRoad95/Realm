namespace RealmCore.Resources.Assets.AssetsTypes;

public interface IFont : IAsset;

public interface IBuiltInFont : IFont;

public interface IAssetFont : IFont
{
    string Path { get; }
}

internal class FontAsset : IAssetFont
{
    public string Name { get; }
    public string Path { get; }
    public string Checksum { get; } = string.Empty;

    public FontAsset(string name, string path)
    {
        Name = name;
        Path = path;
    }
}

internal sealed class BuildInFont : IBuiltInFont
{
    public string Name { get; }
    public string Checksum { get; } = string.Empty;

    public BuildInFont(string name)
    {
        Name = name;
    }
}
