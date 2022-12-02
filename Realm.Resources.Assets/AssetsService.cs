namespace Realm.Resources.Assets;

public class AssetsService
{
    internal event Action<string, IModel>? ModelAdded;
    public AssetsService()
    {

    }

    public void AddModel(string name, IModel model)
    {
        ModelAdded?.Invoke(name, model);
    }
}
