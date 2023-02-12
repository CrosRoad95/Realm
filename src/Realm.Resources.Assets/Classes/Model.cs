using Realm.Resources.Assets.Interfaces;

namespace Realm.Resources.Assets.Classes;

internal class Model : IModel
{
    public string Name { get; }
    public string Path { get; }

    public Model(string name, string path)
    {
        Name = name;
        Path = path;
    }
}
