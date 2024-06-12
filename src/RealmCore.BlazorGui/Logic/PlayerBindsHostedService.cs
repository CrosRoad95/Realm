using static RealmCore.Server.Modules.Users.UsersResults;

namespace RealmCore.BlazorGui.Logic;


internal class CounterPageGui : BrowserGui
{
    public CounterPageGui(RealmPlayer player) : base(player, "counter")
    {
    }
}

internal class HomePageGui : BrowserGui
{
    public HomePageGui(RealmPlayer player) : base(player, "home")
    {
    }
}

internal sealed class PlayerBindsHostedService : IHostedService
{
    public struct GuiOpenOptions
    {
        public List<Type>? allowToOpenWithGui;
    }

    private readonly IUsersService _usersService;
    private readonly IServiceProvider _serviceProvider;

    public PlayerBindsHostedService(IUsersService usersService, IServiceProvider serviceProvider)
    {
        _usersService = usersService;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _usersService.LoggedIn += HandleLoggedIn;
        _usersService.LoggedOut += HandleLoggedOut;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _usersService.LoggedIn += HandleLoggedIn;
        _usersService.LoggedOut += HandleLoggedOut;

        return Task.CompletedTask;
    }

    private Task HandleLoggedIn(RealmPlayer player)
    {
        player.SetBind("num_0", (player, keyState) =>
        {
            if (keyState == KeyState.Up)
                return;

            player.Admin.NoClip = !player.Admin.NoClip;
        });

        player.SetBind("F2", (player, keyState) =>
        {
            if (keyState == KeyState.Up)
                return;

            var browserService = player.Browser;
            if (browserService.Visible)
            {
                browserService.TryClose();
            }
            else
            {
                browserService.Open("/realmUi/counter1");
            }
        });

        player.SetBind("F3", (player, keyState) =>
        {
            if (keyState == KeyState.Up)
                return;

            var browserService = player.Browser;
            if (browserService.Visible)
            {
                browserService.TryClose();
                return;
            }
            browserService.Open("/realmUi/counter2");
        });

        player.SetBind("F4", (player, keyState) =>
        {
            if (keyState == KeyState.Up)
                return;

            var browserService = player.Browser;
            if (browserService.Visible)
            {
                browserService.TryClose();
                return;
            }
            browserService.Open("/realmUi/index");
        });

        GuiHelpers.BindGuiPage<HomePageGui>(player, "F6");
        GuiHelpers.BindGuiPage<CounterPageGui>(player, "F7");

        return Task.CompletedTask;
    }

    private Task HandleLoggedOut(RealmPlayer player)
    {
        player.RemoveAllBinds();
        return Task.CompletedTask;
    }
}
