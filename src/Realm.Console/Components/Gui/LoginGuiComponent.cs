using Realm.Persistance.Extensions;

namespace Realm.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class LoginGuiComponent : GuiComponent
{
    [Inject]
    private IRPGUserManager SignInService { get; set; } = default!;
    [Inject]
    private UserManager<User> UserManager { get; set; } = default!;

    public LoginGuiComponent() : base("login", false)
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
        switch (formContext.FormName)
        {
            case "login":
                var loginData = formContext.GetData<LoginData>();

                var user = await UserManager.Users
                    .IncludeAll()
                    .Where(u => u.UserName == loginData.Login)
                    .AsNoTrackingWithIdentityResolution()
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }

                if(!await UserManager.CheckPasswordAsync(user, loginData.Password))
                {
                    formContext.ErrorResponse("Login lub hasło jest niepoprawne.");
                    return;
                }
                if(await SignInService.SignIn(Entity, user))
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
