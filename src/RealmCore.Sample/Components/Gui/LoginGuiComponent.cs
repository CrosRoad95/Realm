using Microsoft.AspNetCore.Identity;
using RealmCore.Persistence.Data;
using RealmCore.Persistence.Interfaces;
using RealmCore.Server.Components.Players.Abstractions;
using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Console.Components.Gui;

public sealed class LoginGuiComponent : DxGuiComponent
{
    private readonly IUsersService _usersService;
    private readonly ILogger<LoginGuiComponent> _loggerLoginGuiComponent;
    private readonly ILogger<RegisterGuiComponent> _loggerRegisterGuiComponent;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserData> _userManager;

    public LoginGuiComponent(IUsersService usersService, ILogger<LoginGuiComponent> loggerLoginGuiComponent, ILogger<RegisterGuiComponent> loggerRegisterGuiComponent, IUserRepository userRepository, UserManager<UserData> userManager) : base("login", false)
    {
        _usersService = usersService;
        _loggerLoginGuiComponent = loggerLoginGuiComponent;
        _loggerRegisterGuiComponent = loggerRegisterGuiComponent;
        _userRepository = userRepository;
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

                var user = await _userRepository.GetUserByLogin(loginData.Login);

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
                    _loggerLoginGuiComponent.LogHandleError(ex);
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
                Entity.AddComponent(new RegisterGuiComponent(_usersService, _loggerRegisterGuiComponent, _loggerLoginGuiComponent, _userRepository, _userManager));
                Entity.DestroyComponent(this);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
