using RealmCore.Sample.Concepts.Gui;
using RealmCore.Server.Concepts.Gui;
using RealmCore.Server.DTOs;
using RealmCore.Server.Helpers;

namespace RealmCore.Sample.Logic;


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

internal sealed class PlayerBindsLogic
{
    public struct GuiOpenOptions
    {
        public List<Type>? allowToOpenWithGui;
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly IVehicleRepository _vehicleRepository;

    public PlayerBindsLogic(IUsersService usersService, IServiceProvider serviceProvider, IVehicleRepository vehicleRepository)
    {
        _serviceProvider = serviceProvider;
        _vehicleRepository = vehicleRepository;
        usersService.SignedIn += HandleSignedIn;
        usersService.SignedOut += HandleSignedOut;
    }
    
    private void HandleSignedIn(RealmPlayer player)
    {
        player.SetBind("num_0", player =>
        {
            player.Admin.NoClip = player.Admin.NoClip;
        });

        player.SetBind("F2", player =>
        {
            var browserService = player.Browser;
            if (browserService.Visible)
            {
                browserService.Close();
            }
            else
            {
                browserService.Open("/realmUi/counter1");
            }
        });

        player.SetBind("F3", player =>
        {
            var browserService = player.Browser;
            if(browserService.Visible)
            {
                browserService.Close();
                return;
            }
            browserService.Open("/realmUi/counter2");
        });

        player.SetBind("F4", player =>
        {
            var browserService = player.Browser;
            if (browserService.Visible)
            {
                browserService.Close();
                return;
            }
            browserService.Open("/realmUi/index");
        });

        GuiHelpers.BindGuiPage<HomePageGui>(player, "F6");
        GuiHelpers.BindGuiPage<CounterPageGui>(player, "F7");

        GuiHelpers.BindGuiPage(player, "F1", async cancellationToken =>
        {
            DashboardGui.DashboardState state = new();
            state.Money = (double)player.Money.Amount;

            var vehiclesWithModelAndPositionDTos = await _vehicleRepository.GetLightVehiclesByUserId(player.UserId, cancellationToken);
            state.VehicleLightInfos = vehiclesWithModelAndPositionDTos.Select(x => new VehicleLightInfoDTO
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.Position,
            }).ToList();
            state.Counter = 3;
            return new DashboardGui(player, state);
        });
        GuiHelpers.BindGui<InventoryGui>(player, "i");
    }

    private void HandleSignedOut(RealmPlayer player)
    {
        player.RemoveAllBinds();
    }
}
