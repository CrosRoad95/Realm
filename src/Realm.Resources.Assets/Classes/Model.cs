using Realm.Resources.Assets.Interfaces;

namespace Realm.Resources.Assets.Classes;

internal class Model : IModel
{
    public string Name { get; }
    public string ColPath { get; }
    public string DffPath { get; }

    public Model(string name, string colPath, string dffPath)
    {
        Name = name;
        ColPath = colPath;
        DffPath = dffPath;
    }
}
