namespace Realm.Console.Components.Gui;

public sealed class LoginGuiComponent : GuiComponent
{
    [Inject]
    private ISignInService SignInService { get; set; } = default!;
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
                    .Include(u => u.Inventory)
                    .ThenInclude(x => x!.InventoryItems)
                    .FirstOrDefaultAsync(u => u.UserName == loginData.Login);

                if(user == null)
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
                Entity.AddComponent(new RegisterGuiComponent());
                Entity.DestroyComponent(this);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
