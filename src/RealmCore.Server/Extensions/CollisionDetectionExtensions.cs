using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Extensions;

public static class CollisionDetectionExtensions
{
    /// <summary>
    /// Open browser path
    /// </summary>
    /// <typeparam name="TGui"></typeparam>
    /// <param name="collisionDetection"></param>
    /// <param name="path"></param>
    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection, string path)
    {
        collisionDetection.ElementEntered += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.Browser.Visible)
                {
                    player.Browser.Open(path);
                }
            }
        };
        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.Browser.Visible)
                {
                    player.Browser.Close();
                }
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection) where TGui : GuiComponent, new()
    {
        collisionDetection.ElementEntered += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<GuiComponent>() && !player.Browser.Visible)
                    player.AddComponent(new TGui());
            }
        };
        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<TGui>())
                    player.TryDestroyComponent<TGui>();
                player.Browser.Close();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection, Func<RealmPlayer, Task<TGui>> factory, Action<Exception>? exceptionHandler) where TGui : GuiComponent
    {
        collisionDetection.ElementEntered += async (element) =>
        {
            try
            {
                if (element is RealmPlayer player)
                {
                    if (!player.HasComponent<GuiComponent>() && !player.Browser.Visible)
                        player.AddComponent(await factory(player));
                }
            }
            catch (Exception ex)
            {
                exceptionHandler?.Invoke(ex);
            }
        };
        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<TGui>())
                    player.TryDestroyComponent<TGui>();
                player.Browser.Close();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection, Func<RealmPlayer, TGui> factory) where TGui : GuiComponent
    {
        collisionDetection.ElementEntered += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<GuiComponent>() && !player.Browser.Visible)
                    player.AddComponent(factory(player));
            }
        };
        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<TGui>())
                    player.TryDestroyComponent<TGui>();
                player.Browser.Close();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection, Func<TGui> factory) where TGui : GuiComponent
    {
        collisionDetection.ElementEntered += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<GuiComponent>() && !player.Browser.Visible)
                    player.AddComponent(factory());
            }
        };
        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<TGui>())
                    player.TryDestroyComponent<TGui>();
                player.Browser.Close();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui1, TGui2>(this ICollisionDetection collisionDetection)
        where TGui1 : GuiComponent, new()
        where TGui2 : GuiComponent, new()
    {
        collisionDetection.ElementEntered += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<GuiComponent>() && !player.Browser.Visible)
                {
                    player.AddComponent(new TGui1());
                    player.AddComponent(new TGui2());
                }
            }
        };

        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (player.HasComponent<TGui1>())
                    player.DestroyComponent<TGui1>();
                if (player.HasComponent<TGui2>())
                    player.DestroyComponent<TGui2>();
                player.Browser.Close();
            }
        };
    }
}
