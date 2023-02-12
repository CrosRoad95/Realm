using Realm.Resources.Assets.Classes;
using Realm.Resources.Assets.Interfaces;
using SlipeServer.Server.Elements.Enums;

namespace Realm.Resources.Assets;

public class AssetsRegistry
{
    private readonly Dictionary<string, IAsset> _assets = new();
    public IReadOnlyDictionary<string, IAsset> Assets => _assets;

    internal IAsset InternalGetAsset(string name)
    {
        if (!_assets.ContainsKey(name))
        {
            throw new Exception($"Asset '{name}' not found.");
        }
        return _assets[name];
    }

    public IAsset GetAsset(string assetName)
    {
        return InternalGetAsset(assetName);
    }

    public TAsset GetAsset<TAsset>(string assetName) where TAsset : IAsset
    {
        return (TAsset)InternalGetAsset(assetName);
    }

    public void AddModel(string name, byte[] col, byte[] dff)
    {
        _assets.Add(name, new Model(name, ""));
    }

    public void AddModel(string name, (byte[], byte[]) colDff)
    {
        AddModel(name, colDff.Item1, colDff.Item2);
    }

    public void AddFont(string name, string path)
    {
        _assets.Add(name, new Font(name, path));
    }

}
