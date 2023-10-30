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

    public PlayerBindsLogic(IEntityEngine entityEngine, IServiceProvider serviceProvider, IVehicleRepository vehicleRepository) : base(entityEngine)
    {
        _serviceProvider = serviceProvider;
        _vehicleRepository = vehicleRepository;
    }

    protected override void ComponentAdded(UserComponent userComponent)
    {
        var entity = userComponent.Entity;
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();

        playerElementComponent.SetBindAsync("num_0", entity =>
        {
            if (entity.TryGetComponent(out AdminComponent adminComponent))
            {
                adminComponent.NoClip = !adminComponent.NoClip;
            }
            return Task.CompletedTask;
        });

        playerElementComponent.SetBind("F2", entity =>
        {
            if (entity.TryGetComponent(out BrowserComponent browserComponent))
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

        playerElementComponent.SetBind("F3", entity =>
        {
            if (entity.TryGetComponent(out BrowserComponent browserComponent))
            {
                browserComponent.Path = "/realmUi/counter1";
                browserComponent.Visible = true;
            }
        });

        playerElementComponent.SetBind("F4", entity =>
        {
            if (entity.TryGetComponent(out BrowserComponent browserComponent))
            {
                browserComponent.Path = "index";
                browserComponent.Visible = false;
            }
        });

        GuiHelpers.BindGuiPage<HomePageComponent>(entity, "F6", _serviceProvider);
        GuiHelpers.BindGuiPage<CounterPageComponent>(entity, "F7", _serviceProvider);

        //playerElementComponent.SetBind("F6", entity =>
        //{
        //    if (entity.TryGetComponent(out BlazorGuiComponent blazorGuiComponent))
        //    {
        //        blazorGuiComponent.Open("home");
        //    }
        //});

        //playerElementComponent.SetBind("F7", entity =>
        //{
        //    if (entity.TryGetComponent(out BlazorGuiComponent blazorGuiComponent))
        //    {
        //        blazorGuiComponent.Open("counter");
        //    }
        //});

        GuiHelpers.BindGuiPage(entity, "F1", async () =>
        {
            DashboardGuiComponent.DashboardState state = new();
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
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
        GuiHelpers.BindGui<InventoryGuiComponent>(entity, "i", _serviceProvider);
    }

    protected override void ComponentDetached(UserComponent userComponent)
    {
        var entity = userComponent.Entity;
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.RemoveAllBinds();
    }
}
