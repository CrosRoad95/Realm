namespace Realm.Domain.Contextes;

public interface IFormContext
{
    string FormName { get; }

    TData GetData<TData>() where TData : ILuaValue, new();
    void SuccessResponse(params object[] data);
    void ErrorResponse(params object[] data);
}
