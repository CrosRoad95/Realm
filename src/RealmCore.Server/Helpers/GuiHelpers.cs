namespace RealmCore.Server.Helpers;

public static class GuiHelpers
{
    #region Non-Cef gui
    public static void BindGui<TGuiComponent>(Entity entity, string bind) where TGuiComponent : GuiComponent, new()
    {
        BindGui(entity, bind, () => new TGuiComponent());
    }

    public static void BindGui<TGuiComponent>(Entity entity, string bind, Func<TGuiComponent> factory) where TGuiComponent : GuiComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBind(bind, entity =>
        {
            if (entity.HasComponent<TGuiComponent>())
            {
                entity.TryDestroyComponent<TGuiComponent>();
                return;
            }

            entity.TryDestroyComponent<GuiComponent>();

            try
            {
                var guiComponent = entity.AddComponent(factory());
                playerElementComponent.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                entity._serviceProvider.GetRequiredService<ILogger<TGuiComponent>>().LogError(ex, "Error while opening gui page");
                entity._serviceProvider.GetRequiredService<ChatBox>().OutputTo(entity, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }
    
    public static void BindGui<TGuiComponent>(Entity entity, string bind, Func<Task<TGuiComponent>> factory) where TGuiComponent : GuiComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBindAsync(bind, async entity =>
        {
            if (entity.HasComponent<TGuiComponent>())
            {
                entity.TryDestroyComponent<TGuiComponent>();
                return;
            }

            entity.TryDestroyComponent<GuiComponent>();

            try
            {
                var guiComponent = entity.AddComponent(await factory());
                playerElementComponent.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                entity._serviceProvider.GetRequiredService<ILogger<TGuiComponent>>().LogError(ex, "Error while opening gui page");
                entity._serviceProvider.GetRequiredService<ChatBox>().OutputTo(entity, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }

    #endregion

    #region CEF Gui
    public static void BindGuiPage<TGuiComponent>(Entity entity, string bind) where TGuiComponent : GuiPageComponent, new()
    {
        BindGuiPage(entity, bind, () => new TGuiComponent());
    }

    public static void BindGuiPage<TGuiComponent>(Entity entity, string bind, Func<TGuiComponent> factory) where TGuiComponent : GuiPageComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBind(bind, entity =>
        {
            if (entity.HasComponent<TGuiComponent>())
            {
                entity.DestroyComponent<TGuiComponent>();
                return;
            }

            entity.TryDestroyComponent<GuiPageComponent>();

            try
            {
                var guiComponent = entity.AddComponent(factory());
                playerElementComponent.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                entity._serviceProvider.GetRequiredService<ILogger<TGuiComponent>>().LogError(ex, "Error while opening gui page");
                entity._serviceProvider.GetRequiredService<ChatBox>().OutputTo(entity, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }

    public static void BindGuiPage<TGuiComponent>(Entity entity, string bind, Func<Task<TGuiComponent>> factory) where TGuiComponent : GuiComponent
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

            try
            {
                entity.AddComponent(await factory());
                playerElementComponent.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                entity._serviceProvider.GetRequiredService<ILogger<TGuiComponent>>().LogError(ex, "Error while opening gui page");
                entity._serviceProvider.GetRequiredService<ChatBox>().OutputTo(entity, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }
    #endregion
}
