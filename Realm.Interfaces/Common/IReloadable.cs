namespace Realm.Interfaces.Common;

public interface IReloadable
{
    void Reload();
    int GetPriority();
}
