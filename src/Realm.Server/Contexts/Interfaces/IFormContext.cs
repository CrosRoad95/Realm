namespace Realm.Server.Contexts.Interfaces;

public interface IFormContext
{
    string FormName { get; }
    Entity Entity { get; }

    TData GetData<TData>() where TData : ILuaValue, new();
    void SuccessResponse(params object[] data);
    void ErrorResponse(params object[] data);
}
