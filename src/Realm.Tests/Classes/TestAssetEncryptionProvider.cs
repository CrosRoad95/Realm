using Realm.Resources.Assets.Interfaces;

namespace Realm.Tests.Classes;

internal class TestAssetEncryptionProvider : IAssetEncryptionProvider
{
    public byte[] Key => new byte[] { };

    public byte[] Encrypt(byte[] data)
    {
        return data;
    }
}
