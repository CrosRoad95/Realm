using RealmCore.Server.DTOs;
using RealmCore.Server.Helpers;

namespace RealmCore.Sample.Logic;


internal class CounterPageComponent : BrowserGuiComponent
{
    public CounterPageComponent() : base("counter")
    {
    }
}

internal class HomePageComponent : BrowserGuiComponent
{
    public HomePageComponent() : base("home")
    {
    }
}

internal sealed class PlayerBindsLogic : ComponentLogic<UserComponent>
{
    public struct GuiOpenOptions
    {
        public List<Type>? allowToOpenWithGui;
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly IVehicleRepository _vehicleRepository;

    public PlayerBindsLogic(IElementFactory elementFactory, IServiceProvider serviceProvider, IVehicleRepository vehicleRepository) : base(elementFactory)
    {
        _serviceProvider = serviceProvider;
        _vehicleRepository = vehicleRepository;
    }

    protected override void ComponentAdded(UserComponent userComponent)
    {
        var player = (RealmPlayer)userComponent.Element;

        player.SetBindAsync("num_0", player =>
        {
            if (player.TryGetComponent(out AdminComponent adminComponent))
            {
                adminComponent.NoClip = !adminComponent.NoClip;
            }
            return Task.CompletedTask;
        });

        player.SetBind("F2", player =>
        {
            if (player.TryGetComponent(out BrowserComponent browserComponent))
            {
                if (browserComponent.Visible)
                {
                    browserComponent.Close();
                }
                else
                {
                    browserComponent.Path = "/realmUi/counter1";
                    browserComponent.Visible = true;
                }
            }
        });

        player.SetBind("F3", player =>
        {
            if (player.TryGetComponent(out BrowserComponent browserComponent))
            {
                browserComponent.Path = "/realmUi/counter1";
                browserComponent.Visible = true;
            }
        });

        player.SetBind("F4", player =>
        {
            if (player.TryGetComponent(out BrowserComponent browserComponent))
            {
                browserComponent.Path = "index";
                browserComponent.Visible = false;
            }
        });

        GuiHelpers.BindGuiPage<HomePageComponent>(player, "F6", _serviceProvider);
        GuiHelpers.BindGuiPage<CounterPageComponent>(player, "F7", _serviceProvider);

        GuiHelpers.BindGuiPage(player, "F1", async () =>
        {
            DashboardGuiComponent.DashboardState state = new();
            if (player.TryGetComponent(out MoneyComponent moneyComponent))
                state.Money = (double)moneyComponent.Money;

            var vehiclesWithModelAndPositionDTos = await _vehicleRepository.GetLightVehiclesByUserId(userComponent.Id);
            state.VehicleLightInfos = vehiclesWithModelAndPositionDTos.Select(x => new VehicleLightInfoDTO
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.Position,
            }).ToList();
            state.Counter = 3;
            return new DashboardGuiComponent(state);
        }, _serviceProvider);
        GuiHelpers.BindGui<InventoryGuiComponent>(player, "i", _serviceProvider);
    }

    protected override void ComponentDetached(UserComponent userComponent)
    {
        var player = (RealmPlayer)userComponent.Element;
        player.RemoveAllBinds();
    }
}
