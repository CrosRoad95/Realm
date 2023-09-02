using RealmCore.Server.DTOs;
using RealmCore.Server.Helpers;
using RealmCore.ECS.Components;
using RealmCore.ECS;
using RealmCore.Server.Components.Players.Abstractions;
using RealmCore.Persistence.Interfaces;

namespace RealmCore.Console.Logic;


internal class CounterPageComponent : GuiPageComponent
{
    public CounterPageComponent() : base("counter", true)
    {
    }
}

internal class HomePageComponent : GuiPageComponent
{
    public HomePageComponent() : base("home", true)
    {
    }
}

internal sealed class PlayerBindsLogic
{
    public struct GuiOpenOptions
    {
        public List<Type>? allowToOpenWithGui;
    }

    private readonly IEntityEngine _ecs;
    private readonly IServiceProvider _serviceProvider;
    private readonly IVehicleRepository _vehicleRepository;

    public PlayerBindsLogic(IEntityEngine ecs, IServiceProvider serviceProvider, IVehicleRepository vehicleRepository)
    {
        _ecs = ecs;
        _serviceProvider = serviceProvider;
        _vehicleRepository = vehicleRepository;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.HasComponent<PlayerTagComponent>())
            entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is UserComponent userComponent)
        {
            var entity = component.Entity;
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
                if (entity.TryGetComponent(out BrowserComponent blazorGuiComponent))
                {
                    if (blazorGuiComponent.Visible)
                    {
                        blazorGuiComponent.Close();
                    }
                    else
                    {
                        blazorGuiComponent.Open("counter");
                    }
                }
            });

            playerElementComponent.SetBind("F3", entity =>
            {
                if (entity.TryGetComponent(out BrowserComponent blazorGuiComponent))
                {
                    if(blazorGuiComponent.Visible)
                    {
                        blazorGuiComponent.Visible = false;
                    }
                    else
                    {
                        blazorGuiComponent.Visible = true;
                        blazorGuiComponent.Path = "fetchdata";
                    }
                }
            });

            playerElementComponent.SetBind("F4", entity =>
            {
                if (entity.TryGetComponent(out BrowserComponent blazorGuiComponent))
                {
                    blazorGuiComponent.Path = "index";
                    blazorGuiComponent.Visible = false;
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
    }
}
