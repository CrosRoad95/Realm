namespace RealmCore.Resources.Assets.Interfaces;

public interface IAssetDFF : IAsset
{
    string Path { get; }
}

public interface IAssetCOL : IAsset
{
    string Path { get; }
}

internal class AssetDFF : IAssetDFF
{
    public string Path { get; }
    public string Name { get; }
    public string Checksum { get; }

    public AssetDFF(string name, string path, string checksum)
    {
        Name = name;
        Path = path;
        Checksum = checksum;
    }
}

internal class AssetCOL : IAssetCOL
{
    public string Path { get; }
    public string Name { get; }
    public string Checksum { get; }

    public AssetCOL(string name, string path, string checksum)
    {
        Name = name;
        Path = path;
        Checksum = checksum;
    }
}
