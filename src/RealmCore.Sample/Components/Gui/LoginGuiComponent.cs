using RealmCore.Sample.Data;
using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Sample.Components.Gui;

public sealed class LoginGuiComponent : DxGuiComponent
{
    private readonly IUsersService _usersService;
    private readonly ILogger<LoginGuiComponent> _logger;
    private readonly UserManager<UserData> _userManager;

    public LoginGuiComponent(IUsersService usersService, ILogger<LoginGuiComponent> logger, UserManager<UserData> userManager) : base("login", false)
    {
        _usersService = usersService;
        _logger = logger;
        _userManager = userManager;
    }

    protected override async Task HandleForm(IFormContext formContext)
    {
        switch (formContext.FormName)
        {
            case "login":
                LoginData loginData;
                try
                {
                    loginData = formContext.GetData<LoginData>();
                }
                catch (ValidationException ex)
                {
                    formContext.ErrorResponse(ex.Errors.First().ErrorMessage);
                    return;
                }

                var user = await _userManager.GetUserByUserName(loginData.Login);

                if (user == null)
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                if (!await _userManager.CheckPasswordAsync(user, loginData.Password))
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                try
                {
                    var player = (RealmPlayer)Element;
                    if (await _usersService.SignIn(player, user))
                    {
                        player.Components.TryDestroyComponent(this);
                        formContext.SuccessResponse();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogHandleError(ex);
                    formContext.ErrorResponse("Błąd podczas logowania.");
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        switch (actionContext.ActionName)
        {
            case "navigateToRegister":
                var player = ((RealmPlayer)Element);
                player.AddComponentWithDI<RegisterGuiComponent>();
                player.DestroyComponent(this);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
