using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Extensions;

public static class PickupElementComponentExtensions
{
    public static void AddOpenGuiLogic<TGui>(this PickupElementComponent pickupElementComponent) where TGui : GuiComponent, new()
    {
        pickupElementComponent.EntityEntered = (enteredPickup, entity) =>
        {
            if (!entity.HasComponent<GuiComponent>())
                entity.AddComponent(new TGui());
        };
        pickupElementComponent.EntityLeft = (leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }

    public static void AddOpenGuiLogic<TGui1, TGui2>(this PickupElementComponent pickupElementComponent)
        where TGui1 : GuiComponent, new()
        where TGui2 : GuiComponent, new()
    {
        pickupElementComponent.EntityEntered = (enteredPickup, entity) =>
        {
            if (!entity.HasComponent<GuiComponent>())
            {
                entity.AddComponent(new TGui1());
                entity.AddComponent(new TGui2());
            }
        };
        pickupElementComponent.EntityLeft = (leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui1>())
                entity.DestroyComponent<TGui1>();
            if (entity.HasComponent<TGui2>())
                entity.DestroyComponent<TGui2>();
        };
    }

    public static void AddOpenGuiPageLogic<TGui>(this PickupElementComponent pickupElementComponent) where TGui : BrowserGuiComponent, new()
    {
        pickupElementComponent.EntityEntered = (enteredPickup, entity) =>
        {
            if (!entity.HasComponent<BrowserGuiComponent>())
                entity.AddComponent(new TGui());
        };
        pickupElementComponent.EntityLeft = (leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }
    
    public static void AddOpenGuiPageLogic<TGui>(this PickupElementComponent pickupElementComponent, Func<TGui> factory) where TGui : BrowserGuiComponent, new()
    {
        pickupElementComponent.EntityEntered = (enteredPickup, entity) =>
        {
            if (!entity.HasComponent<BrowserGuiComponent>())
                entity.AddComponent(factory());
        };
        pickupElementComponent.EntityLeft = (leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }
    
    public static void AddAsyncOpenGuiPageLogic<TGui>(this PickupElementComponent pickupElementComponent, Func<Task<TGui>> factory) where TGui : BrowserGuiComponent, new()
    {
        pickupElementComponent.EntityEntered = async (enteredPickup, entity) =>
        {
            if (!entity.HasComponent<BrowserGuiComponent>())
                entity.AddComponent(await factory());
        };
        pickupElementComponent.EntityLeft = (leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }

}
