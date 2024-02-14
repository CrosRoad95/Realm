using RealmCore.Resources.Assets.Interfaces;

namespace RealmCore.TestingTools.Classes;

internal class TestAssetEncryptionProvider : IAssetEncryptionProvider
{
    public byte[] Key => new byte[] { };

    public byte[] Encrypt(byte[] data)
    {
        return data;
    }
}
