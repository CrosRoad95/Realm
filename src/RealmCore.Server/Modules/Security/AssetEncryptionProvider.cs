namespace RealmCore.Server.Modules.Security;

internal class AssetEncryptionProvider : IAssetEncryptionProvider
{
    private readonly IOptions<AssetsOptions> _assetsOptions;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly byte[] _key;
    private readonly AESCrypto _aesCrypto;
    private readonly IReadOnlySet<string> _excludeExtensions;

    public string Key { get; }

    public AssetEncryptionProvider(IOptions<AssetsOptions> assetsOptions, IHostEnvironment hostEnvironment)
    {
        _assetsOptions = assetsOptions;
        _hostEnvironment = hostEnvironment;
        Key = assetsOptions.Value.Base64Key;
        _key = Convert.FromBase64String(assetsOptions.Value.Base64Key);
        _aesCrypto = new AESCrypto(_key, _key, true);
        _excludeExtensions = assetsOptions.Value.ExcludeExtensions.ToHashSet();
    }

    public bool ShouldEncryptByExtension(string extension) => IsEncryptionEnabled() && !_excludeExtensions.Contains(extension);

    public bool IsEncryptionEnabled() => _hostEnvironment.IsProduction() || _assetsOptions.Value.AlwaysEncryptModels;

    public byte[] Encrypt(byte[] data) => _aesCrypto.PerformAES(data);

    public string TryEncryptPath(string path)
    {
        if (ShouldEncryptByExtension(Path.GetExtension(path)))
        {
            return path.CalculateChecksum();
        }
        return path;
    }
}
