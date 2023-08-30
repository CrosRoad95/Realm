using RealmCore.Resources.GuiSystem;

namespace RealmCore.Server.Logic.Components;

internal sealed class GuiSystemServiceLogic : ComponentLogic<GuiComponent>
{
    private readonly IEntityEngine _entityEngine;
    private readonly IGuiSystemService? _guiSystemService;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly ILogger<GuiSystemServiceLogic> _logger;
    private readonly IServiceProvider _serviceProvider;
    private ConcurrentDictionary<string, GuiComponent> _guiComponents = new();

    public GuiSystemServiceLogic(IEntityEngine entityEngine, FromLuaValueMapper fromLuaValueMapper, ILogger<GuiSystemServiceLogic> logger, IServiceProvider serviceProvider, IGuiSystemService? guiSystemService = null) : base(entityEngine)
    {
        _entityEngine = entityEngine;
        _fromLuaValueMapper = fromLuaValueMapper;
        _logger = logger;
        _serviceProvider = serviceProvider;
        if (guiSystemService != null)
        {
            guiSystemService.FormSubmitted += HandleFormSubmitted;
            guiSystemService.ActionExecuted += HandleActionExecuted;
            _guiSystemService = guiSystemService;
        }
    }

    protected override void ComponentAdded(GuiComponent component)
    {
        if(_guiSystemService != null)
        {
            _guiSystemService.OpenGui(component.Entity.GetPlayer(), component.Name, component.Cursorless);
            _guiComponents.TryAdd(component.Name, component);
        }
    }

    protected override void ComponentDetached(GuiComponent component)
    {
        if (_guiSystemService != null)
        {
            _guiSystemService.CloseGui(component.Entity.GetPlayer(), component.Name, component.Cursorless);
            _guiComponents.TryRemove(component.Name, out var _);
        }
    }

    private async void HandleActionExecuted(LuaEvent luaEvent)
    {
        if (_guiSystemService != null)
        {
            try
            {
                var (id, guiName, actionName, data) = luaEvent.Read<string, string, string, LuaValue>(_fromLuaValueMapper);
                if (_guiComponents.TryGetValue(guiName, out GuiComponent guiComponent))
                {
                    try
                    {
                        await guiComponent.InternalHandleAction(new ActionContext(actionName, data));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to execute action {actionName}.", actionName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read luaEvent parameters");
            }
        }
    }

    private async void HandleFormSubmitted(LuaEvent luaEvent)
    {
        if (_guiSystemService != null)
        {
            try
            {
                var (id, guiName, formName, data) = luaEvent.Read<string, string, string, LuaValue>(_fromLuaValueMapper);
                if (_guiComponents.TryGetValue(guiName, out GuiComponent guiComponent))
                {
                    var formContext = new FormContext(luaEvent.Player, formName, data, _guiSystemService, _entityEngine, _serviceProvider);
                    try
                    {
                        await guiComponent.InternalkHandleForm(formContext);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogHandleError(ex);
                        formContext.ErrorResponse("Wystąpił nieznany błąd.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read luaEvent parameters");
            }
        }
    }
}
