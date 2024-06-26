﻿namespace RealmCore.Server.Modules.Security;

internal class AssetEncryptionProvider : IAssetEncryptionProvider
{
    private readonly AESCrypto _aesCrypto;
    private readonly byte[] _key;

    public byte[] Key => _key;

    private HashSet<string> _excludeExtensions =  ["otf", "ttf"];

    public AssetEncryptionProvider(IOptions<AssetsOptions> assetsOptions)
    {
        _key = Convert.FromBase64String(assetsOptions.Value.Base64Key);

        _aesCrypto = new AESCrypto(_key, _key, true);
    }

    public bool ShouldEncryptByExtension(string extension) => _excludeExtensions.Contains(extension);

    public byte[] Encrypt(byte[] data) => _aesCrypto.PerformAES(data);
}
