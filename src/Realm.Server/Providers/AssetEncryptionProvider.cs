using Grpc.Core;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Realm.Domain.Options;
using Realm.Resources.Assets.Interfaces;

namespace Realm.Server.Providers;

internal class AssetEncryptionProvider : IAssetEncryptionProvider
{
    private readonly AESCrypto _aesCrypto;
    private readonly byte[] _key;

    public byte[] Key => _key;

    public AssetEncryptionProvider(IOptions<AssetsOptions> assetsOptions)
    {
        _key = System.Convert.FromBase64String(assetsOptions.Value.Base64Key);

        _aesCrypto = new AESCrypto(_key, _key, true);
    }


    public byte[] Encrypt(byte[] data) => _aesCrypto.PerformAES(data);
}
