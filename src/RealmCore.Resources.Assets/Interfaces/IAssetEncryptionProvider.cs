namespace RealmCore.Resources.Assets.Interfaces;

public interface IAssetEncryptionProvider
{
    byte[] Encrypt(byte[] data);
    bool ShouldEncryptByExtension(string extension);
    string EncryptPath(string path);
    bool IsEncryptionEnabled();

    string Key { get; }
}
