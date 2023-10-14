using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Helpers;

public static class GuiHelpers
{
    #region Non-Browser gui
    public static void BindGui<TGuiComponent>(Entity entity, string bind, IServiceProvider serviceProvider) where TGuiComponent : GuiComponent, new()
    {
        BindGui(entity, bind, () => new TGuiComponent(), serviceProvider);
    }

    public static void BindGui<TGuiComponent>(Entity entity, string bind, Func<TGuiComponent> factory, IServiceProvider serviceProvider) where TGuiComponent : GuiComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        var logger = serviceProvider.GetRequiredService<ILogger<TGuiComponent>>();
        var chatBox = serviceProvider.GetRequiredService<ChatBox>();
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
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(entity, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }
    
    public static void BindGui<TGuiComponent>(Entity entity, string bind, Func<Task<TGuiComponent>> factory, IServiceProvider serviceProvider) where TGuiComponent : GuiComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        var logger = serviceProvider.GetRequiredService<ILogger<TGuiComponent>>();
        var chatBox = serviceProvider.GetRequiredService<ChatBox>();
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
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(entity, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }

    #endregion

    #region Browser Gui
    public static void BindGuiPage<TGuiComponent>(Entity entity, string bind, IServiceProvider serviceProvider) where TGuiComponent : GuiBlazorComponent, new()
    {
        BindGuiPage(entity, bind, () => new TGuiComponent(), serviceProvider);
    }

    public static void BindGuiPage<TGuiComponent>(Entity entity, string bind, Func<TGuiComponent> factory, IServiceProvider serviceProvider) where TGuiComponent : GuiBlazorComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        var logger = serviceProvider.GetRequiredService<ILogger<TGuiComponent>>();
        var chatBox = serviceProvider.GetRequiredService<ChatBox>();
        playerElementComponent.SetBind(bind, entity =>
        {
            if (entity.HasComponent<TGuiComponent>())
            {
                entity.DestroyComponent<TGuiComponent>();
                return;
            }

            entity.TryDestroyComponent<GuiBlazorComponent>();

            try
            {
                var guiComponent = entity.AddComponent(factory());
                playerElementComponent.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(entity, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }

    public static void BindGuiPage<TGuiComponent>(Entity entity, string bind, Func<Task<TGuiComponent>> factory, IServiceProvider serviceProvider) where TGuiComponent : GuiComponent
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        var logger = serviceProvider.GetRequiredService<ILogger<TGuiComponent>>();
        var chatBox = serviceProvider.GetRequiredService<ChatBox>();
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
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(entity, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }
    #endregion
}
