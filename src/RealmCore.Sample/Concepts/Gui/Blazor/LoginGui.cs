using Discord;
using Microsoft.AspNetCore.Identity;
using RealmCore.Server.Modules.Users;
using Serilog;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace RealmCore.Sample.Concepts.Gui.Blazor;

public class LoginGui : BrowserGui
{
    private readonly UserManager<UserData> _userManager;
    private readonly IUsersService _usersService;

    public LoginGui(PlayerContext playerContext, UserManager<UserData> userManager, IUsersService usersService) : base(playerContext.Player, "/realmUi/login", new Dictionary<string, string?>
    {
        ["initialCounter"] = "1337"
    })
    {
        _userManager = userManager;
        _usersService = usersService;
    }

    public async Task Login()
    {
        var user = await Player.GetRequiredService<IPlayerUserService>().GetUserByUserName("admin") ?? throw new Exception("User not found");
        if(user != null)
            await _usersService.LogIn(Player, user);
    }
}
