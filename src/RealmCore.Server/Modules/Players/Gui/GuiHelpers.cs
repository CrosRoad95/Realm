﻿namespace RealmCore.Server.Modules.Players.Gui;

public static class GuiHelpers
{
    public static void BindGuiPage<TGui>(RealmPlayer player, string bind) where TGui : BrowserGui
    {
        BindGuiPage(player, bind, () => ActivatorUtilities.CreateInstance<TGui>(player.ServiceProvider));
    }

    public static void BindGuiPage<TGui>(RealmPlayer player, string bind, Func<TGui> factory) where TGui : BrowserGui
    {
        var logger = player.GetRequiredService<ILogger<TGui>>();
        var chatBox = player.GetRequiredService<ChatBox>();
        player.SetBind(bind, (player, keyState) =>
        {
            if (keyState == KeyState.Up)
                return;

            if (player.Gui.Current is TGui)
            {
                player.Gui.Current = null;
                return;
            }

            try
            {
                player.Gui.Current = factory();
                player.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(player, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }

    public static void BindGuiPage<TGui>(RealmPlayer player, string bind, Func<CancellationToken, Task<TGui>> factory) where TGui : IPlayerGui
    {
        var logger = player.GetRequiredService<ILogger<TGui>>();
        var chatBox = player.GetRequiredService<ChatBox>();
        player.SetBindAsync(bind, async (player, keyState, token) =>
        {
            if (keyState == KeyState.Up)
                return;

            if (player.Gui.Current is TGui)
            {
                player.Gui.Current = null;
                return;
            }

            try
            {
                player.Gui.Current = await factory(token);
                player.ResetCooldown(bind);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while opening gui page");
                chatBox.OutputTo(player, "Wystąpił bład podczas próby otwarcia gui");
            }
        });
    }
}
