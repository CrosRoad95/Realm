namespace Realm.Resources.Assets.Interfaces;

public interface IModel : IAsset
{
    string ColPath { get; }
    string DffPath { get; }
}
