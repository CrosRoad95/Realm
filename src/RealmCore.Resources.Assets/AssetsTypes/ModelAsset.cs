namespace RealmCore.Resources.Assets.AssetsTypes;

public interface IAssetDFF : IAsset
{
    string Path { get; }
}

public interface IAssetCOL : IAsset
{
    string Path { get; }
}

public interface IAssetTXD : IAsset
{
    string Path { get; }
}

internal sealed class AssetDFF : IAssetDFF
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

    public override string ToString() => "Model";
}

internal sealed class AssetCOL : IAssetCOL
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

    public override string ToString() => "Collision";
}

internal sealed class AssetTXD : IAssetTXD
{
    public string Path { get; }
    public string Name { get; }
    public string Checksum { get; }

    public AssetTXD(string name, string path, string checksum)
    {
        Name = name;
        Path = path;
        Checksum = checksum;
    }

    public override string ToString() => "Textures";
}
