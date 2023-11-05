using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Extensions;

public static class PickupElementComponentExtensions
{
    public static void AddOpenGuiLogic<TGui>(this RealmPickup pickup) where TGui : GuiComponent, new()
    {
        pickup.CollisionDetection.Entered += (enteredPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (!components.HasComponent<GuiComponent>())
                    components.AddComponent(new TGui());
            }
        };
        pickup.CollisionDetection.Left += (leftPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (components.HasComponent<TGui>())
                    components.DestroyComponent<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui1, TGui2>(this RealmPickup pickup)
        where TGui1 : GuiComponent, new()
        where TGui2 : GuiComponent, new()
    {
        pickup.CollisionDetection.Entered += (enteredPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (!components.HasComponent<GuiComponent>())
                {
                    components.AddComponent(new TGui1());
                    components.AddComponent(new TGui2());
                }
            }
        };
        pickup.CollisionDetection.Left += (leftPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (components.HasComponent<TGui1>())
                    components.DestroyComponent<TGui1>();
                if (components.HasComponent<TGui2>())
                    components.DestroyComponent<TGui2>();
            }
        };
    }

    public static void AddOpenGuiPageLogic<TGui>(this RealmPickup pickup) where TGui : BrowserGuiComponent, new()
    {
        pickup.CollisionDetection.Entered += (enteredPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (!components.HasComponent<BrowserGuiComponent>())
                    components.AddComponent(new TGui());
            }
        };
        pickup.CollisionDetection.Left += (leftPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (components.HasComponent<TGui>())
                    components.DestroyComponent<TGui>();
            }
        };
    }

    public static void AddOpenGuiPageLogic<TGui>(this RealmPickup pickup, Func<TGui> factory) where TGui : BrowserGuiComponent, new()
    {
        pickup.CollisionDetection.Entered += (enteredPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (!components.HasComponent<BrowserGuiComponent>())
                    components.AddComponent(factory());
            }
        };
        pickup.CollisionDetection.Left += (leftPickup, element) =>
        {
            if (element is IComponents components)
            {

                if (components.HasComponent<TGui>())
                    components.DestroyComponent<TGui>();
            }
        };
    }

    public static void AddAsyncOpenGuiPageLogic<TGui>(this RealmPickup pickup, Func<Task<TGui>> factory) where TGui : BrowserGuiComponent, new()
    {
        pickup.CollisionDetection.Entered += async (enteredPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (!components.HasComponent<BrowserGuiComponent>())
                    components.AddComponent(await factory());
            }
        };
        pickup.CollisionDetection.Left += (leftPickup, element) =>
        {
            if (element is IComponents components)
            {
                if (components.HasComponent<TGui>())
                    components.DestroyComponent<TGui>();
            }
        };
    }

}
