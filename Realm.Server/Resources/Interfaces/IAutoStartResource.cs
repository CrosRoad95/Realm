namespace Realm.Server.Resources.Interfaces;

public interface IAutoStartResource
{
    void StartFor(IResourceProvider resourceProvider, Player player);
}
