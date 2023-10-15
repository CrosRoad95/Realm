namespace RealmCore.Server.Contexts;

internal class FormContext : IFormContext
{
    private bool _responsed = false;
    private readonly Player _player;
    private readonly string _formName;
    private readonly LuaValue _data;
    private readonly IGuiSystemService _GuiSystemService;
    private readonly IEntityEngine _ecs;
    private readonly IServiceProvider _serviceProvider;

    public string FormName => _formName;

    public Entity Entity
    {
        get
        {
            _ecs.TryGetEntityByPlayer(_player, out var entity);
            return entity ?? throw new InvalidOperationException();
        }
    }

    public FormContext(Player player, string formName, LuaValue data, IGuiSystemService GuiSystemService, IEntityEngine ecs, IServiceProvider serviceProvider)
    {
        _player = player;
        _formName = formName;
        _data = data;
        _GuiSystemService = GuiSystemService;
        _ecs = ecs;
        _serviceProvider = serviceProvider;
    }

    public TData GetData<TData>(bool supressValidation = false) where TData : ILuaValue, new()
    {
        var data = new TData();
        data.Parse(_data);
        if(!supressValidation)
        {
            var validator = _serviceProvider.GetRequiredService<IValidator<TData>>();
            validator.ValidateAndThrow(data);
        }
        return data;
    }

    public void SuccessResponse(params object[] data)
    {
        if (_responsed)
            throw new Exception("Form already got response.");

        _GuiSystemService.SendFormResponse(_player, "", FormName, true, data);
        _responsed = true;
    }

    public void ErrorResponse(params object[] data)
    {
        if (_responsed)
            throw new Exception("Form already got response.");

        _GuiSystemService.SendFormResponse(_player, "", FormName, false, data);
        _responsed = true;
    }
}
