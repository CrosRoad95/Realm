using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Helpers;

public static class GuiHelpers
{
    #region Non-Browser gui
    public static void BindGui<TGuiComponent>(RealmPlayer player, string bind, IServiceProvider serviceProvider) where TGuiComponent : GuiComponent, new()
    {
        BindGui(player, bind, () => new TGuiComponent(), serviceProvider);
    }

    public static void BindGui<TGuiComponent>(RealmPlayer player, string bind, Func<TGuiComponent> factory, IServiceProvider serviceProvider) where TGuiComponent : GuiComponent
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TGuiComponent>>();
        var chatBox = serviceProvider.GetRequiredService<ChatBox>();
        player.SetBind(bind, player =>
        {
            if (player.HasComponent<TGuiComponent>())
            {
                player.TryDestroyComponent<TGuiComponent>();
                return;
            }
            else
                player.TryDestroyComponent<GuiComponent>();

            try
            {
                var guiComponent = player.AddComponent(factory());
                player.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(player, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }
    
    public static void BindGui<TGuiComponent>(RealmPlayer player, string bind, Func<Task<TGuiComponent>> factory, IServiceProvider serviceProvider) where TGuiComponent : GuiComponent
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TGuiComponent>>();
        var chatBox = serviceProvider.GetRequiredService<ChatBox>();
        player.SetBindAsync(bind, async player =>
        {
            var components = player;
            if (components.HasComponent<TGuiComponent>())
            {
                components.TryDestroyComponent<TGuiComponent>();
                return;
            }
            else
                components.TryDestroyComponent<GuiComponent>();

            try
            {
                var guiComponent = components.AddComponent(await factory());
                player.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(player, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }

    #endregion

    #region Browser Gui
    public static void BindGuiPage<TGuiComponent>(RealmPlayer player, string bind, IServiceProvider serviceProvider) where TGuiComponent : BrowserGuiComponent, new()
    {
        BindGuiPage(player, bind, () => new TGuiComponent(), serviceProvider);
    }

    public static void BindGuiPage<TGuiComponent>(RealmPlayer player, string bind, Func<TGuiComponent> factory, IServiceProvider serviceProvider) where TGuiComponent : BrowserGuiComponent
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TGuiComponent>>();
        var chatBox = serviceProvider.GetRequiredService<ChatBox>();
        player.SetBind(bind, player =>
        {
            var components = player;
            if (components.HasComponent<TGuiComponent>())
            {
                components.DestroyComponent<TGuiComponent>();
                return;
            }
            else
                components.TryDestroyComponent<BrowserGuiComponent>();

            try
            {
                var guiComponent = components.AddComponent(factory());
                player.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(player, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }

    public static void BindGuiPage<TGuiComponent>(RealmPlayer player, string bind, Func<Task<TGuiComponent>> factory, IServiceProvider serviceProvider) where TGuiComponent : GuiComponent
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TGuiComponent>>();
        var chatBox = serviceProvider.GetRequiredService<ChatBox>();
        player.SetBindAsync(bind, async player =>
        {
            var components = player;
            if (components.HasComponent<TGuiComponent>())
            {
                components.DestroyComponent<TGuiComponent>();
                return;
            }
            else
                components.TryDestroyComponent<GuiComponent>();

            try
            {
                components.AddComponent(await factory());
                player.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(player, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }
    #endregion
}
