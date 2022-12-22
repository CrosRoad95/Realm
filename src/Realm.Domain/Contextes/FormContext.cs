using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;

namespace Realm.Domain.Contextes;

internal class FormContext : IFormContext
{
    private readonly string _formName;
    private readonly LuaValue _data;

    public string FormName => _formName;
    public FormContext(string formName, LuaValue data)
    {
        _formName = formName;
        _data = data;
    }

    public TData GetData<TData>() where TData : ILuaValue, new()
    {
        var data = new TData();
        data.Parse(_data);
        return data;
    }
}
