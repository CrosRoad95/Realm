using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class LoginGuiComponent : GuiComponent
{
    [Inject]
    private IUsersService RPGUserManager { get; set; } = default!;
    [Inject]
    private ILogger<GuiComponent> Logger { get; set; } = default!;

    public LoginGuiComponent() : base("login", false)
    {

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
                catch(ValidationException ex)
                {
                    formContext.ErrorResponse(ex.Errors.First().ErrorMessage);
                    return;
                }

                var user = await RPGUserManager.GetUserByLogin(loginData.Login);

                if (user == null)
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                if (!await RPGUserManager.IsSerialWhitelisted(user.Id, formContext.Entity.GetRequiredComponent<PlayerElementComponent>().Client.Serial))
                {
                    Logger.LogWarning("Player logged in to not whitelisted user.");
                }

                if (!await RPGUserManager.CheckPasswordAsync(user, loginData.Password))
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                try
                {
                    if (await RPGUserManager.SignIn(Entity, user))
                    {
                        Entity.TryDestroyComponent(this);
                        formContext.SuccessResponse();
                        return;
                    }
                }
                catch(Exception)
                {
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
                Entity.AddComponent<RegisterGuiComponent>();
                Entity.DestroyComponent(this);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
