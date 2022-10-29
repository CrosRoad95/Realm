namespace Realm.Interfaces.Common;

public interface IReloadable
{
    Task Reload();
    int GetPriority();
}
