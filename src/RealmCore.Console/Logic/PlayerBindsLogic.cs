using RealmCore.Server.DTOs;
using RealmCore.Server.Components;
using RealmCore.Server.Enums;
using RealmCore.Server.Helpers;

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

    private readonly IECS _ecs;
    private readonly IVehiclesService _vehiclesService;

    public PlayerBindsLogic(IECS ecs, IVehiclesService vehiclesService)
    {
        _ecs = ecs;
        _vehiclesService = vehiclesService;
        _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if (entity.Tag == EntityTag.Player)
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
                if (entity.TryGetComponent(out BlazorGuiComponent blazorGuiComponent))
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
                if (entity.TryGetComponent(out BlazorGuiComponent blazorGuiComponent))
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
                if (entity.TryGetComponent(out BlazorGuiComponent blazorGuiComponent))
                {
                    blazorGuiComponent.Path = "index";
                    blazorGuiComponent.Visible = false;
                }
            });

            GuiHelpers.BindGuiPage<HomePageComponent>(entity, "F6");
            GuiHelpers.BindGuiPage<CounterPageComponent>(entity, "F7");

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

                var vehiclesWithModelAndPositionDTos = await _vehiclesService.GetLightVehiclesByUserId(userComponent.Id);
                state.VehicleLightInfos = vehiclesWithModelAndPositionDTos.Select(x => new VehicleLightInfoDTO
                {
                    Id = x.Id,
                    Model = x.Model,
                    Position = x.Position,
                }).ToList();
                state.Counter = 3;
                return new DashboardGuiComponent(state);
            });
            GuiHelpers.BindGui<InventoryGuiComponent>(entity, "i");
        }
    }
}
