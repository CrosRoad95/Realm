using RealmCore.Resources.Assets.Classes;
using SlipeServer.Server.Enums;

namespace RealmCore.Resources.Assets;

public record struct ReplacedModel(int model, string dffPath, string colPath);

public class AssetsCollection
{
    private readonly object _lock = new();
    private readonly string _basePath = "Server/Assets";
    private readonly Dictionary<string, IAsset> _assets = [];
    private readonly Dictionary<ObjectModel, ReplacedModel> _replacedModels = [];
    public IReadOnlyDictionary<string, IAsset> Assets => _assets;
    public IReadOnlyDictionary<ObjectModel, ReplacedModel> ReplacedModels => _replacedModels;

    public AssetsCollection()
    {
        if (Directory.Exists(_basePath))
        {
            foreach (var item in Directory.GetFiles(_basePath, "*.*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetRelativePath(_basePath, item);

                switch (Path.GetDirectoryName(fileName))
                {
                    case "Fonts":
                        AddFont(Path.GetFileNameWithoutExtension(fileName), fileName);
                        break;
                }
            }
        }
    }

    private void CreateDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (directory == null || Directory.Exists(directory))
            return;

        Directory.CreateDirectory(directory);
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

    public IAssetDFF AddProceduralDFF(string name, Stream stream)
    {
        lock (_lock)
        {
            if (_assets.ContainsKey(name))
                throw new Exception($"Name '{name}' already in use");

            var fileName = Path.Combine(_basePath, $"Models/Procedural/{name}.dff");

            var data = stream.ToArray();
            var checksum = data.CalculateChecksum();

            if (File.Exists(fileName))
                File.Delete(fileName);

            File.WriteAllBytes(fileName, data);

            return new AssetDFF(name, fileName, checksum);
        }
    }

    public IAssetCOL AddProceduralCOL(string name, Stream stream)
    {
        lock (_lock)
        {
            if (_assets.ContainsKey(name))
                throw new Exception($"Name '{name}' already in use");

            var fileName = Path.Combine(_basePath, $"Models/Procedural/{name}.col");

            var data = stream.ToArray();
            var checksum = data.CalculateChecksum();

            if (File.Exists(fileName))
                File.Delete(fileName);

            File.WriteAllBytes(fileName, data);

            return new AssetCOL(name, fileName, checksum);
        }
    }

    public void AddFont(string name, string path)
    {
        lock (_lock)
        {
            var fullPath = Path.Combine(_basePath, path);
            CreateDirectory(fullPath);
            _assets.Add(name, new FileSystemFont(name, fullPath));
        }
    }

    public void ReplaceModel(ObjectModel objectModel, IAssetDFF dff, IAssetCOL col)
    {
        lock (_lock)
            _replacedModels[objectModel] = new ReplacedModel((int)objectModel, dff.Path, col.Path);
    }

    internal IEnumerable<string> GetAllFiles()
    {
        lock (_lock)
        {
            foreach (var item in _assets)
            {
                switch (item.Value)
                {
                    case IAssetDFF assetDFF:
                        yield return assetDFF.Path;
                        break;
                    case IAssetCOL assetCOL:
                        yield return assetCOL.Path;
                        break;
                    case IAssetFont font:
                        yield return font.Path;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
