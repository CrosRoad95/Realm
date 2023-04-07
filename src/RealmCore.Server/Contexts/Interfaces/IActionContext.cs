namespace RealmCore.Server.Contexts.Interfaces;

public interface IActionContext
{
    string ActionName { get; }

    TData GetData<TData>() where TData : ILuaValue, new();
}
