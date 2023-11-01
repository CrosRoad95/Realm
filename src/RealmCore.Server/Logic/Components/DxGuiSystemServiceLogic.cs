using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Logic.Components;

internal sealed class DxGuiSystemServiceLogic : ComponentLogic<DxGuiComponent>
{
    private readonly IElementFactory _elementFactory;
    private readonly IGuiSystemService? _guiSystemService;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly ILogger<DxGuiSystemServiceLogic> _logger;
    private readonly IServiceProvider _serviceProvider;
    private ConcurrentDictionary<string, DxGuiComponent> _guiComponents = new();

    public DxGuiSystemServiceLogic(IElementFactory elementFactory, FromLuaValueMapper fromLuaValueMapper, ILogger<DxGuiSystemServiceLogic> logger, IServiceProvider serviceProvider, IGuiSystemService? guiSystemService = null) : base(elementFactory)
    {
        _elementFactory = elementFactory;
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

    protected override void ComponentAdded(DxGuiComponent dxGuiComponent)
    {
        if(_guiSystemService != null)
        {
            _guiSystemService.OpenGui((RealmPlayer)dxGuiComponent.Element, dxGuiComponent.Name, dxGuiComponent.Cursorless);
            _guiComponents.TryAdd(dxGuiComponent.Name, dxGuiComponent);
        }
    }

    protected override void ComponentDetached(DxGuiComponent dxGuiComponent)
    {
        if (_guiSystemService != null)
        {
            var player = (RealmPlayer)dxGuiComponent.Element;
            _guiSystemService.CloseGui(player, dxGuiComponent.Name, dxGuiComponent.Cursorless);
            _guiComponents.TryRemove(dxGuiComponent.Name, out var _);
        }
    }

    private async void HandleActionExecuted(LuaEvent luaEvent)
    {
        if (_guiSystemService != null)
        {
            try
            {
                var (id, guiName, actionName, data) = luaEvent.Read<string, string, string, LuaValue>(_fromLuaValueMapper);
                if (_guiComponents.TryGetValue(guiName, out DxGuiComponent dxGuiComponent))
                {
                    try
                    {
                        await dxGuiComponent.InternalHandleAction(new ActionContext(actionName, data));
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
                if (_guiComponents.TryGetValue(guiName, out DxGuiComponent dxGuiComponent))
                {
                    var formContext = new FormContext((RealmPlayer)luaEvent.Player, formName, data, _guiSystemService, _elementFactory, _serviceProvider);
                    try
                    {
                        await dxGuiComponent.InternalHandleForm(formContext);
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
