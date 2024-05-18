using RealmCore.Sample.Data;

namespace RealmCore.Sample.Concepts.Gui.Dx;

public sealed class RegisterDxGui : DxGui, IGuiHandlers
{
    private readonly IUsersService _usersService;
    private readonly UserManager<UserData> _userManager;

    public RegisterDxGui(IUsersService usersService, UserManager<UserData> userManager, PlayerContext playerContext) : base(playerContext.Player, "register", false)
    {
        _usersService = usersService;
        _userManager = userManager;
    }

    public async Task HandleForm(IFormContext formContext)
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
                    var userId = await _usersService.Register(registerData.Login, registerData.Password);

                    formContext.Player.Gui.SetCurrentWithDI<LoginGui>();
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

    public Task HandleAction(IActionContext actionContext)
    {
        switch (actionContext.ActionName)
        {
            case "navigateToLogin":
                actionContext.Player.Gui.SetCurrentWithDI<LoginGui>();
                break;
            default:
                throw new NotImplementedException();
        }

        return Task.CompletedTask;
    }
}
