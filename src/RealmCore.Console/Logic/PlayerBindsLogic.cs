using RealmCore.Server.DTOs;
using RealmCore.Server.Components;
using RealmCore.Server.Enums;

namespace RealmCore.Console.Logic;

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

    private void OpenCloseGuiHelper<TGuiComponent>(Entity entity, string bind, GuiOpenOptions options = default) where TGuiComponent : GuiComponent, new()
    {
        OpenCloseGuiHelper(entity, bind, () => new TGuiComponent(), options);
    }

    private void OpenCloseGuiHelper<TGuiComponent>(Entity entity, string bind, Func<TGuiComponent> factory, GuiOpenOptions options = default) where TGuiComponent : GuiComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBindAsync(bind, async entity =>
        {
            if (entity.HasComponent<TGuiComponent>())
            {
                entity.DestroyComponent<TGuiComponent>();
                return;
            }

            if (entity.HasComponent<GuiComponent>())
                entity.DestroyComponent<GuiComponent>();

            var guiComponent = entity.AddComponent(factory());
            playerElementComponent.ResetCooldown(bind);
        });
    }

    private void OpenCloseGuiHelper<TGuiComponent>(Entity entity, string bind, Func<Task<TGuiComponent>> factory, GuiOpenOptions options = default) where TGuiComponent : GuiComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBindAsync(bind, async entity =>
        {
            if (entity.HasComponent<TGuiComponent>())
            {
                entity.DestroyComponent<TGuiComponent>();
                return;
            }

            entity.TryDestroyComponent<GuiComponent>();

            var guiComponent = entity.AddComponent(await factory());
            playerElementComponent.ResetCooldown(bind);
        });
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

            OpenCloseGuiHelper(entity, "F1", async () =>
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
                return new DashboardGuiComponent(state);
            });
            OpenCloseGuiHelper<InventoryGuiComponent>(entity, "i");
        }
    }
}
