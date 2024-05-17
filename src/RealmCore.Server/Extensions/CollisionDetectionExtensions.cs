namespace RealmCore.Server.Extensions;

public static class CollisionDetectionExtensions
{
    /// <summary>
    /// Open browser path
    /// </summary>
    /// <typeparam name="TGui"></typeparam>
    /// <param name="collisionShape"></param>
    /// <param name="path"></param>
    public static void AddOpenGuiLogic<TGui>(this CollisionShape collisionShape, string path)
    {
        collisionShape.ElementEntered += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                if (!player.Browser.Visible)
                {
                    player.Browser.Open(path);
                }
            }
        };
        collisionShape.ElementLeft += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                if (!player.Browser.Visible)
                {
                    player.Browser.TryClose();
                }
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this CollisionShape collisionShape) where TGui : IPlayerGui
    {
        collisionShape.ElementEntered += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                if (player.Gui.Current == null)
                    player.Gui.SetCurrentWithDI<TGui>();
            }
        };
        collisionShape.ElementLeft += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                player.Gui.TryClose<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this CollisionShape collisionShape, Func<RealmPlayer, Task<TGui>> factory, Action<Exception>? exceptionHandler) where TGui : IPlayerGui
    {
        collisionShape.ElementEntered += async (collisionShape, args) =>
        {
            try
            {
                if (args.Element is RealmPlayer player)
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
        collisionShape.ElementLeft += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                player.Gui.TryClose<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this CollisionShape collisionShape, Func<RealmPlayer, TGui> factory) where TGui : IPlayerGui
    {
        collisionShape.ElementEntered += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                if (player.Gui.Current == null)
                    player.Gui.Current = factory(player);
            }
        };
        collisionShape.ElementLeft += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                player.Gui.TryClose<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this CollisionShape collisionShape, Func<TGui> factory) where TGui : IPlayerGui
    {
        collisionShape.ElementEntered += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                if (player.Gui.Current == null)
                    player.Gui.Current = factory();
            }
        };
        collisionShape.ElementLeft += (collisionShape, args) =>
        {
            if (args.Element is RealmPlayer player)
            {
                player.Gui.TryClose<TGui>();
            }
        };
    }
}
