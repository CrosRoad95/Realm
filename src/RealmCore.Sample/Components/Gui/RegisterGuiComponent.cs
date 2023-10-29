using RealmCore.Sample.Data;
using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Sample.Components.Gui;

public sealed class RegisterGuiComponent : DxGuiComponent
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUsersService _usersService;
    private readonly UserManager<UserData> _userManager;

    public RegisterGuiComponent(IServiceProvider serviceProvider, IUsersService usersService, UserManager<UserData> userManager) : base("register", false)
    {
        _serviceProvider = serviceProvider;
        _usersService = usersService;
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

                if (await _userManager.IsUserNameInUse(registerData.Login))
                {
                    formContext.ErrorResponse("Login zajęty.");
                    return;
                }

                try
                {
                    var userId = await _usersService.SignUp(registerData.Login, registerData.Password);

                    var scope = _serviceProvider.CreateScope();
                    Entity.AddComponent<LoginGuiComponent>(scope.ServiceProvider);
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
                var scope = _serviceProvider.CreateScope();
                Entity.AddComponentWithDI<LoginGuiComponent>();
                Entity.DestroyComponent(this);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
