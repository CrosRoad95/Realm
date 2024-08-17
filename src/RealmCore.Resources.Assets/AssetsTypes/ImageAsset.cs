namespace RealmCore.Resources.Assets.AssetsTypes;

public interface IImageAsset : IAsset
{
    string Path { get; }
}

public interface IRemoteImageAsset : IImageAsset
{
    string FullPath { get; }
}

internal sealed class RemoteImageAsset : IRemoteImageAsset
{
    public string Path { get; }
    public string FullPath { get; }
    public string Name { get; }
    public string Checksum { get; }

    public RemoteImageAsset(string path, string fullPath, string name, string checksum)
    {
        Path = path;
        FullPath = fullPath;
        Name = name;
        Checksum = checksum;
    }
}
