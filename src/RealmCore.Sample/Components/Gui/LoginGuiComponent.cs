using RealmCore.Sample.Data;
using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Sample.Components.Gui;

public sealed class LoginGuiComponent : DxGuiComponent
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUsersService _usersService;
    private readonly ILogger<LoginGuiComponent> _logger;
    private readonly UserManager<UserData> _userManager;

    public LoginGuiComponent(IServiceProvider serviceProvider, IUsersService usersService, ILogger<LoginGuiComponent> logger, UserManager<UserData> userManager) : base("login", false)
    {
        _serviceProvider = serviceProvider;
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

                var user = await _userManager.GetUserByLogin(loginData.Login);

                if (user == null)
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                //if (!await usersService.IsSerialWhitelisted(user.Id, formContext.Entity.GetRequiredComponent<PlayerElementComponent>().Client.Serial))
                //{
                //    Logger.LogWarning("Player logged in to not whitelisted user.");
                //    formContext.ErrorResponse("Nie możesz zalogować się na to konto.");
                //    return;
                //}

                if (!await _userManager.CheckPasswordAsync(user, loginData.Password))
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                try
                {
                    if (await _usersService.SignIn(Entity, user))
                    {
                        Entity.TryDestroyComponent(this);
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
                Entity.AddComponentWithDI<RegisterGuiComponent>();
                Entity.DestroyComponent(this);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
