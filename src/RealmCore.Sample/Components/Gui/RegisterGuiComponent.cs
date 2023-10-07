using RealmCore.Server.Components.Players.Abstractions;
using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Console.Components.Gui;

public sealed class RegisterGuiComponent : DxGuiComponent
{
    private readonly IUsersService _usersService;
    private readonly ILogger<RegisterGuiComponent> _loggerRegisterGuiWindow;
    private readonly ILogger<LoginGuiComponent> _loggerLoginGuiWindow;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserData> _userManager;

    public RegisterGuiComponent(IUsersService usersService, ILogger<RegisterGuiComponent> loggerRegisterGuiWindow, ILogger<LoginGuiComponent> loggerLoginGuiWindow, IUserRepository userRepository, UserManager<UserData> userManager) : base("register", false)
    {
        _usersService = usersService;
        _loggerRegisterGuiWindow = loggerRegisterGuiWindow;
        _loggerLoginGuiWindow = loggerLoginGuiWindow;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    protected override async Task HandleForm(IFormContext formContext)
    {
        switch (formContext.FormName)
        {
            case "register":
                var registerData = formContext.GetData<RegisterData>();
                if (registerData.Password != registerData.RepeatPassword)
                {
                    formContext.ErrorResponse("Podane hasła nie są takie same.");
                    return;
                }

                if (await _userRepository.IsUserNameInUse(registerData.Login))
                {
                    formContext.ErrorResponse("Login zajęty.");
                    return;
                }

                try
                {
                    var userId = await _usersService.SignUp(registerData.Login, registerData.Password);
                    Entity.AddComponent(new LoginGuiComponent(_usersService, _loggerLoginGuiWindow, _loggerRegisterGuiWindow, _userRepository, _userManager));
                    Entity.DestroyComponent(this);
                }
                catch (Exception ex)
                {
                    formContext.ErrorResponse("Wystąpił błąd podczas rejestracji.");
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
            case "navigateToLogin":
                Entity.AddComponent(new LoginGuiComponent(_usersService, _loggerLoginGuiWindow, _loggerRegisterGuiWindow, _userRepository, _userManager));
                Entity.DestroyComponent(this);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
