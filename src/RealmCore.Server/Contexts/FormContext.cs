namespace RealmCore.Server.Contexts;

internal class FormContext : IFormContext
{
    private bool _responsed = false;
    private readonly Player _player;
    private readonly string _formName;
    private readonly LuaValue _data;
    private readonly IAgnosticGuiSystemService _agnosticGuiSystemService;
    private readonly IECS _ecs;

    public string FormName => _formName;

    public Entity Entity
    {
        get
        {
            _ecs.TryGetEntityByPlayer(_player, out var entity);
            return entity;
        }
    }

    public FormContext(Player player, string formName, LuaValue data, IAgnosticGuiSystemService agnosticGuiSystemService, IECS ecs)
    {
        _player = player;
        _formName = formName;
        _data = data;
        _agnosticGuiSystemService = agnosticGuiSystemService;
        _ecs = ecs;
    }

    public TData GetData<TData>() where TData : ILuaValue, new()
    {
        var data = new TData();
        data.Parse(_data);
        return data;
    }

    public void SuccessResponse(params object[] data)
    {
        if (_responsed)
            throw new Exception("Form already got response.");

        _agnosticGuiSystemService.SendFormResponse(_player, "", FormName, true, data);
        _responsed = true;
    }

    public void ErrorResponse(params object[] data)
    {
        if (_responsed)
            throw new Exception("Form already got response.");

        _agnosticGuiSystemService.SendFormResponse(_player, "", FormName, false, data);
        _responsed = true;
    }
}
