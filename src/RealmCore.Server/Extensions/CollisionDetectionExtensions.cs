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

    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection) where TGui : IPlayerGui
    {
        collisionDetection.ElementEntered += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (player.Gui.Current == null)
                    player.Gui.SetCurrentWithDI<TGui>();
            }
        };
        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                player.Gui.Close<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection, Func<RealmPlayer, Task<TGui>> factory, Action<Exception>? exceptionHandler) where TGui : IPlayerGui
    {
        collisionDetection.ElementEntered += async (element) =>
        {
            try
            {
                if (element is RealmPlayer player)
                {
                    if (player.Gui.Current == null)
                        player.Gui.Current = await factory(player);
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
                player.Gui.Close<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection, Func<RealmPlayer, TGui> factory) where TGui : IPlayerGui
    {
        collisionDetection.ElementEntered += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (player.Gui.Current == null)
                    player.Gui.Current = factory(player);
            }
        };
        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                player.Gui.Close<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this ICollisionDetection collisionDetection, Func<TGui> factory) where TGui : IPlayerGui
    {
        collisionDetection.ElementEntered += (element) =>
        {
            if (element is RealmPlayer player)
            {
                if (player.Gui.Current == null)
                    player.Gui.Current = factory();
            }
        };
        collisionDetection.ElementLeft += (element) =>
        {
            if (element is RealmPlayer player)
            {
                player.Gui.Close<TGui>();
            }
        };
    }
}
