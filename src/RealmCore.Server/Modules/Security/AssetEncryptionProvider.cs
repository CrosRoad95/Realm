namespace RealmCore.Server.Modules.Security;

internal class AssetEncryptionProvider : IAssetEncryptionProvider
{
    private readonly IOptions<AssetsOptions> _assetsOptions;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly byte[] _key;
    private readonly AESCrypto _aesCrypto;

    public string Key => Convert.ToBase64String(_key);

    private readonly IReadOnlySet<string> _excludeExtensions = new HashSet<string> { "otf", "ttf" };

    public AssetEncryptionProvider(IOptions<AssetsOptions> assetsOptions, IHostEnvironment hostEnvironment)
    {
        _assetsOptions = assetsOptions;
        _hostEnvironment = hostEnvironment;
        _key = Convert.FromBase64String(assetsOptions.Value.Base64Key);
        _aesCrypto = new AESCrypto(_key, _key, true);
    }

    public bool ShouldEncryptByExtension(string extension) => _excludeExtensions.Contains(extension);

    public bool IsEncryptionEnabled() => _hostEnvironment.IsProduction() || _assetsOptions.Value.AlwaysEncryptModels;

    public byte[] Encrypt(byte[] data)
    {
        if(IsEncryptionEnabled())
            return _aesCrypto.PerformAES(data);
        return data;
    }

    public string EncryptPath(string path)
    {
        if(IsEncryptionEnabled())
            return path.CalculateChecksum();
        return path;
    }
}
