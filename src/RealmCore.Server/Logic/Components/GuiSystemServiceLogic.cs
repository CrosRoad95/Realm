using RealmCore.Resources.GuiSystem;
using RealmCore.Server.Components.Elements;
using SlipeServer.Server.Events;

namespace RealmCore.Server.Logic.Components;

internal class GuiSystemServiceLogic : ComponentLogic<GuiComponent>
{
    private readonly IEntityEngine _entityEngine;
    private readonly IGuiSystemService _guiSystemService;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly ILogger<GuiSystemServiceLogic> _logger;
    private readonly IServiceProvider _serviceProvider;
    private ConcurrentDictionary<string, GuiComponent> _guiComponents = new();

    public GuiSystemServiceLogic(IEntityEngine entityEngine, IGuiSystemService guiSystemService, FromLuaValueMapper fromLuaValueMapper, ILogger<GuiSystemServiceLogic> logger, IServiceProvider serviceProvider) : base(entityEngine)
    {
        guiSystemService.FormSubmitted += HandleFormSubmitted;
        guiSystemService.ActionExecuted += HandleActionExecuted;
        _entityEngine = entityEngine;
        _guiSystemService = guiSystemService;
        _fromLuaValueMapper = fromLuaValueMapper;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override void ComponentAdded(GuiComponent component)
    {
        _guiSystemService.OpenGui(component.Entity.GetPlayer(), component.Name, component.Cursorless);
        _guiComponents.TryAdd(component.Name, component);
    }

    protected override void ComponentDetached(GuiComponent component)
    {
        _guiSystemService.CloseGui(component.Entity.GetPlayer(), component.Name, component.Cursorless);
        _guiComponents.TryRemove(component.Name, out var _);
    }

    private async void HandleActionExecuted(LuaEvent luaEvent)
    {
        try
        {
            var (id, guiName, actionName, data) = luaEvent.Read<string, string, string, LuaValue>(_fromLuaValueMapper);
            if(_guiComponents.TryGetValue(guiName, out GuiComponent guiComponent))
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

    private async void HandleFormSubmitted(LuaEvent luaEvent)
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
