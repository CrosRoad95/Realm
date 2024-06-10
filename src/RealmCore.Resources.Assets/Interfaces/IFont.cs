namespace RealmCore.Resources.Assets.Interfaces;

public interface IFont : IAsset;

public interface IBuiltInFont : IFont;

public interface IAssetFont : IFont
{
    string Path { get; }
}
