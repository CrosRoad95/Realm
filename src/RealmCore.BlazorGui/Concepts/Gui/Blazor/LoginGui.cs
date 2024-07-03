namespace RealmCore.BlazorGui.Concepts.Gui.Blazor;

public class LoginGui : BrowserGui
{
    private readonly IUsersService _usersService;

    public LoginGui(PlayerContext playerContext, IUsersService usersService) : base(playerContext.Player, "/realmUi/login", new Dictionary<string, string?>
    {
        ["initialCounter"] = "1337"
    })
    {
        _usersService = usersService;
    }

    public async Task LoginAdmin()
    {
        var user = await Player.GetRequiredService<IPlayerUserService>().GetUserByUserName("admin") ?? throw new Exception("User not found");
        if (user != null)
            await _usersService.LogIn(Player, user);
    }

    public async Task LoginUser()
    {
        var user = await Player.GetRequiredService<IPlayerUserService>().GetUserByUserName("user") ?? throw new Exception("User not found");
        if (user != null)
            await _usersService.LogIn(Player, user);
    }
}
