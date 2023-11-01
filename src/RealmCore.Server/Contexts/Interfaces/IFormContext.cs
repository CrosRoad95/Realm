namespace RealmCore.Server.Contexts.Interfaces;

public interface IFormContext
{
    string FormName { get; }
    RealmPlayer Player { get; }

    TData GetData<TData>(bool supressValidation = false) where TData : ILuaValue, new();
    void SuccessResponse(params object[] data);
    void ErrorResponse(params object[] data);
}
