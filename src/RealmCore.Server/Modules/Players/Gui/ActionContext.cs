namespace RealmCore.Server.Modules.Players.Gui;

public interface IActionContext
{
    string ActionName { get; }
    RealmPlayer Player { get; }

    TData GetData<TData>() where TData : ILuaValue, new();
}

internal class ActionContext : IActionContext
{
    private readonly RealmPlayer _player;
    private readonly string _actionName;
    private readonly LuaValue _data;

    public string ActionName => _actionName;
    public RealmPlayer Player => _player;
    public ActionContext(RealmPlayer player, string formName, LuaValue data)
    {
        _player = player;
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
