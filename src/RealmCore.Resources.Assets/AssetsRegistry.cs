using RealmCore.Resources.Assets.Classes;
using RealmCore.Resources.Assets.Interfaces;
using SlipeServer.Server.Enums;

namespace RealmCore.Resources.Assets;

public class AssetsRegistry : IServerAssetsProvider
{
    private readonly object _lock = new();
    private readonly Dictionary<string, IAsset> _assets = [];
    private readonly Dictionary<ObjectModel, string> _replacedModels = [];
    public IReadOnlyDictionary<string, IAsset> Assets => _assets;
    public IReadOnlyDictionary<ObjectModel, string> ReplacedModels => _replacedModels;

    public AssetsRegistry()
    {
        try
        {
            Directory.CreateDirectory("Server/Assets/Models/Procedural");
        }
        catch(Exception) { }
    }

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
        lock (_lock)
            return InternalGetAsset(assetName);
    }

    public TAsset GetAsset<TAsset>(string assetName) where TAsset : IAsset
    {
        lock (_lock)
            return (TAsset)InternalGetAsset(assetName);
    }

    public IFont GetFont(string assetName)
    {
        lock (_lock)
            return (IFont)InternalGetAsset(assetName);
    }

    public IModel AddModel(string name, Stream colStream, Stream dffStream)
    {
        lock (_lock)
            if (_assets.ContainsKey(name))
                throw new Exception($"Name '{name}' already in use");

        var colFileName = $"Server/Assets/Models/Procedural/{name}.col";
        var dffFileName = $"Server/Assets/Models/Procedural/{name}.dff";

        using (var fileStream = File.Create(colFileName))
            colStream.CopyTo(fileStream);
        using (var fileStream = File.Create(dffFileName))
            dffStream.CopyTo(fileStream);

        var model = new Model(name, colFileName, dffFileName);
        lock (_lock)
            _assets.Add(name, model);
        return model;
    }

    public void AddFont(string name, string path)
    {
        lock (_lock)
            _assets.Add(name, new Font(name, path));
    }

    public void ReplaceModel(ObjectModel objectModel, IModel model)
    {
        lock (_lock)
            _replacedModels[objectModel] = model.Name;
    }

    public IEnumerable<string> Provide()
    {
        lock (_lock)
        {
            foreach (var item in _assets)
            {
                switch (item.Value)
                {
                    case IModel model:
                        yield return model.ColPath;
                        yield return model.DffPath;
                        break;
                    case IFont font:
                        yield return font.FontPath;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
