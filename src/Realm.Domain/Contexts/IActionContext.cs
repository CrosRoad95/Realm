namespace Realm.Domain.Contexts;

public interface IActionContext
{
    string ActionName { get; }

    TData GetData<TData>() where TData : ILuaValue, new();
}
