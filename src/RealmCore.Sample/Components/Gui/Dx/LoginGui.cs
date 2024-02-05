namespace RealmCore.Sample.Components.Gui.Dx;

public sealed class LoginGui : DxGui, IGuiHandlers
{
    private readonly IUsersService _usersService;
    private readonly ILogger<LoginGui> _logger;
    private readonly UserManager<UserData> _userManager;

    public LoginGui(IUsersService usersService, ILogger<LoginGui> logger, UserManager<UserData> userManager, PlayerContext playerContext) : base(playerContext.Player, "login", false)
    {
        _usersService = usersService;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task HandleForm(IFormContext formContext)
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
                    if (await _usersService.SignIn(formContext.Player, user))
                    {
                        formContext.Player.Gui.Current = null;
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

    public async Task HandleAction(IActionContext actionContext)
    {
        switch (actionContext.ActionName)
        {
            case "navigateToRegister":
                actionContext.Player.Gui.SetCurrentWithDI<RegisterDxGui>();
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
