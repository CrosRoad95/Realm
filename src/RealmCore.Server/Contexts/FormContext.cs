namespace RealmCore.Server.Contexts;

internal class FormContext : IFormContext
{
    private bool _responded = false;
    private readonly RealmPlayer _player;
    private readonly string _formName;
    private readonly LuaValue _data;
    private readonly IGuiSystemService _GuiSystemService;

    public string FormName => _formName;

    public RealmPlayer Player => _player;

    public FormContext(RealmPlayer player, string formName, LuaValue data, IGuiSystemService GuiSystemService)
    {
        _player = player;
        _formName = formName;
        _data = data;
        _GuiSystemService = GuiSystemService;
    }

    public TData GetData<TData>(bool supressValidation = false) where TData : ILuaValue, new()
    {
        var data = new TData();
        data.Parse(_data);
        if(!supressValidation)
        {
            var validator = _player.GetRequiredService<IValidator<TData>>();
            validator.ValidateAndThrow(data);
        }
        return data;
    }

    public void SuccessResponse(params object[] data)
    {
        if (_responded)
            throw new Exception("Form already got response.");

        _GuiSystemService.SendFormResponse(_player, "", FormName, true, data);
        _responded = true;
    }

    public void ErrorResponse(params object[] data)
    {
        if (_responded)
            throw new Exception("Form already got response.");

        _GuiSystemService.SendFormResponse(_player, "", FormName, false, data);
        _responded = true;
    }
}
