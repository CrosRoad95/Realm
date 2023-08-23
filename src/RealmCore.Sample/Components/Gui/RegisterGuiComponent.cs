using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class RegisterGuiComponent : GuiComponent
{
    [Inject]
    private IUsersService RPGUserManager { get; set; } = default!;

    public RegisterGuiComponent() : base("register", false)
    {

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

                if (await RPGUserManager.IsUserNameInUse(registerData.Login))
                {
                    formContext.ErrorResponse("Login zajęty.");
                    return;
                }

                try
                {
                    var userId = await RPGUserManager.SignUp(registerData.Login, registerData.Password);
                    Entity.AddComponent<LoginGuiComponent>();
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
                Entity.AddComponent<LoginGuiComponent>();
                Entity.DestroyComponent(this);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
