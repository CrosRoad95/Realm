namespace RealmCore.Resources.Assets.Interfaces;

public interface IAssetEncryptionProvider
{
    byte[] Encrypt(byte[] data);
    bool ShouldEncryptByExtension(string extension);

    byte[] Key { get; }
}
