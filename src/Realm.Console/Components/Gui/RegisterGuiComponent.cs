namespace Realm.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class RegisterGuiComponent : GuiComponent
{
    [Inject]
    private IRPGUserManager SignInService { get; set; } = default!;
    [Inject]
    private UserManager<User> UserManager { get; set; } = default!;

    public RegisterGuiComponent() : base("register", false)
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
        switch (formContext.FormName)
        {
            case "register":
                var registerData = formContext.GetData<RegisterData>();
                if(registerData.Password != registerData.RepeatPassword)
                {
                    formContext.ErrorResponse("Podane hasła nie są takie same.");
                    return;
                }

                var user = await UserManager.Users
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == registerData.Login.ToLower());

                if (user != null)
                {
                    formContext.ErrorResponse("Login zajęty.");
                    return;
                }

                var identityResult = await UserManager.CreateAsync(new User
                {
                    UserName = registerData.Login,
                }, registerData.Password);
                if (identityResult.Succeeded)
                {
                    Entity.AddComponent<LoginGuiComponent>();
                    Entity.DestroyComponent(this);
                    return;
                }
                else
                {
                    formContext.ErrorResponse(identityResult.Errors.First().Description);
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
