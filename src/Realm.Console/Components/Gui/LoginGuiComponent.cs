using Realm.Domain.Contexts;

namespace Realm.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class LoginGuiComponent : GuiComponent
{
    [Inject]
    private IRPGUserManager RPGUserManager { get; set; } = default!;
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
                var loginData = formContext.GetData<LoginData>();

                var user = await RPGUserManager.GetUserByLogin(loginData.Login);

                if (user == null)
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                if (!await RPGUserManager.IsSerialWhitelisted(user.Id, formContext.Entity.GetRequiredComponent<PlayerElementComponent>().Client.Serial))
                {
                    Logger.LogWarning("Player logged in to not whitelisted user account.");
                }

                if(!await RPGUserManager.CheckPasswordAsync(user, loginData.Password))
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                if (await RPGUserManager.SignIn(Entity, user))
                {
                    Entity.DestroyComponent(this);
                    formContext.SuccessResponse();
                    return;
                }
                formContext.ErrorResponse("Błąd podczas logowania.");
                break;
            default:
                throw new NotImplementedException();
        }
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        switch(actionContext.ActionName)
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
