using Realm.Server.Contexts.Interfaces;

namespace Realm.Server.Contexts;

internal class ActionContext : IActionContext
{
    private readonly string _actionName;
    private readonly LuaValue _data;

    public string ActionName => _actionName;
    public ActionContext(string formName, LuaValue data)
    {
        _actionName = formName;
        _data = data;
    }

    public TData GetData<TData>() where TData : ILuaValue, new()
    {
        var data = new TData();
        data.Parse(_data);
        return data;
    }
}
