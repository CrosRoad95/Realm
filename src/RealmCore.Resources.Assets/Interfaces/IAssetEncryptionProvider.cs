namespace RealmCore.Resources.Assets.Interfaces;

public interface IAssetEncryptionProvider
{
    byte[] Encrypt(byte[] data);
    byte[] Key { get; }
}
