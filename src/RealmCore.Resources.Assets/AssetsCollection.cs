using RenderWareBuilders;

namespace RealmCore.Resources.Assets;

public class AssetsCollection
{
    private readonly object _lock = new();
    private readonly string _basePath = "Server/Assets";
    private readonly string _modelsBasePath = "Models";
    private readonly Dictionary<string, IAsset> _assets = [];

    public string BasePath => _basePath;

    public IReadOnlyDictionary<string, IAsset> Assets
    {
        get
        {
            IReadOnlyDictionary<string, IAsset> view;
            lock (_lock)
                view = new Dictionary<string, IAsset>(_assets);
            return view;
        }
    }

    public AssetsCollection()
    {
        if (!Directory.Exists(_basePath))
            return;

        TxdBuilder? txdBuilder = null;

        foreach (var fullFileName in Directory.GetFiles(_basePath, "*.*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetRelativePath(_basePath, fullFileName);
            var name = Path.GetFileNameWithoutExtension(fileName);
            var directoryName = Path.GetDirectoryName(fileName);
            if(directoryName == _modelsBasePath)
            {
                switch (Path.GetExtension(fileName))
                {
                    case ".obj":
                        var wavefrontLoader = new RenderWareIo.Wavefront.WavefrontLoader(fullFileName);
                        var groups = wavefrontLoader.GetAllGroups();
                        var options = new RenderWareIo.Wavefront.WavefrontLoaderOptions
                        {
                            DayNightColor = new RenderWareBuilders.DayNightColors
                            {
                                day = System.Drawing.Color.FromArgb(100, 100, 100),
                                night = System.Drawing.Color.FromArgb(45, 45, 45),
                            },
                            //CollisionMaterials = groups.ToDictionary(wavefrontLoader.GetGroupTextureName, y => MaterialId.WoodBench)
                        };

                        {
                            var dff = wavefrontLoader.CreateDff(groups, options);
                            using var stream = new MemoryStream();
                            dff.Write(stream);
                            stream.Position = 0;
                            AddDFF($"{name}Model", stream);
                        }

                        {
                            var col = wavefrontLoader.CreateCol(groups, options);
                            using var stream = new MemoryStream();
                            col.Write(stream);
                            stream.Position = 0;
                            AddCOL($"{name}Collision", stream);
                        }
                        break;
                }
            }
            else if(directoryName == "ModelsTextures")
            {
                txdBuilder ??= new TxdBuilder();
                txdBuilder.AddImage(name, fullFileName);
            }
            else if(directoryName == "Fonts")
            {
                AddFont(name, fileName);
            }
        }

        if (txdBuilder != null)
        {
            using var stream = new MemoryStream();
            var txd = txdBuilder.Build();
            txd.Write(stream);
            stream.Position = 0;
            AddTXD("ModelsTextures", stream);
        }
    }

    private void CreateDirectoryForFile(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (directory == null || Directory.Exists(directory))
            return;

        Directory.CreateDirectory(directory);
    }

    internal IAsset InternalGetAsset(string name)
    {
        if (!_assets.TryGetValue(name, out IAsset? value))
        {
            throw new Exception($"Asset '{name}' not found.");
        }
        return value;
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
    
    public IRemoteImageAsset GetDynamicRemoteImage(string url)
    {
        return new RemoteImageAsset(url, url, Path.GetFileNameWithoutExtension(url), "");
    }

    public IFont GetFont(string assetName)
    {
        lock (_lock)
            return (IFont)InternalGetAsset(assetName);
    }

    public (IAssetDFF,IAssetCOL) AddModelFromFile(string name, string dffFileName, string colFileName)
    {
        lock (_lock)
        {
            if (_assets.ContainsKey(name))
                throw new Exception($"Name '{name}' already in use");

            dffFileName = Path.ChangeExtension(Path.Join(_modelsBasePath, dffFileName), ".dff").Replace("\\", "/");
            colFileName = Path.ChangeExtension(Path.Join(_modelsBasePath, colFileName), ".col").Replace("\\", "/");

            var dffFullFileName = Path.Combine(_basePath, dffFileName).Replace("\\", "/");
            var colFullFileName = Path.Combine(_basePath, colFileName).Replace("\\", "/");
            var dffChecksum = File.ReadAllBytes(dffFullFileName).CalculateChecksum();
            var colChecksum = File.ReadAllBytes(colFullFileName).CalculateChecksum();
            var assetDFF = new AssetDFF(name, dffFileName, dffChecksum);
            _assets.Add($"{name}Model", assetDFF);
            var assetCOL = new AssetCOL(name, colFileName, colChecksum);
            _assets.Add($"{name}Collision", assetCOL);
            return (assetDFF, assetCOL);
        }

    }

    public IAssetDFF AddDFF(string name, Stream stream)
    {
        lock (_lock)
        {
            if (_assets.ContainsKey(name))
                throw new Exception($"Name '{name}' already in use");

            var fileName = $"{_modelsBasePath}/{name}.dff";
            var fullFileName = Path.Combine(_basePath, fileName);

            CreateDirectoryForFile(fullFileName);

            var data = stream.ToArray();
            var checksum = data.CalculateChecksum();

            if (File.Exists(fullFileName))
                File.Delete(fullFileName);

            File.WriteAllBytes(fullFileName, data);

            var asset = new AssetDFF(name, fileName, checksum);
            _assets.Add(name, asset);
            return asset;
        }
    }

    public IAssetCOL AddCOL(string name, Stream stream)
    {
        lock (_lock)
        {
            if (_assets.ContainsKey(name))
                throw new Exception($"Name '{name}' already in use");

            var fileName = $"{_modelsBasePath}/{name}.col";
            var fullFileName = Path.Combine(_basePath, fileName);

            CreateDirectoryForFile(fullFileName);

            var data = stream.ToArray();
            var checksum = data.CalculateChecksum();

            if (File.Exists(fullFileName))
                File.Delete(fullFileName);

            File.WriteAllBytes(fullFileName, data);

            var asset = new AssetCOL(name, fileName, checksum);
            _assets.Add(name, asset);
            return asset;
        }
    }

    public IAssetTXD AddTXD(string name, Stream stream)
    {
        lock (_lock)
        {
            if (_assets.ContainsKey(name))
                throw new Exception($"Name '{name}' already in use");

            var fileName = $"{_modelsBasePath}/{name}.txd";
            var fullFileName = Path.Combine(_basePath, fileName);

            CreateDirectoryForFile(fullFileName);

            var data = stream.ToArray();
            var checksum = data.CalculateChecksum();

            if (File.Exists(fullFileName))
                File.Delete(fullFileName);

            File.WriteAllBytes(fullFileName, data);

            var asset = new AssetTXD(name, fileName, checksum);
            _assets.Add(name, asset);
            return asset;
        }
    }

    public void AddFont(string name, string path)
    {
        lock (_lock)
        {
            var fullPath = Path.Combine(_basePath, path);
            CreateDirectoryForFile(fullPath);
            _assets.Add(name, new FontAsset(name, path));
        }
    }

    public async Task AddRemoteImage(HttpClient httpClient, string path)
    {
        // Check if file exists
        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, path));
        response.EnsureSuccessStatusCode();

        var fullPath = Path.Join(httpClient.BaseAddress!.ToString(), path);
        lock (_lock)
        {
            _assets.Add(path, new RemoteImageAsset(path, fullPath, Path.GetFileNameWithoutExtension(path), ""));
        }
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
                    case IAssetTXD assetTXD:
                        yield return assetTXD.Path;
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
