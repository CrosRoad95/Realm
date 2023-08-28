using RealmCore.ECS;

namespace RealmCore.Server.Extensions;

public static class MarkerElementComponentExtensions
{
    public static void AddOpenGuiLogic<TGui>(this MarkerElementComponent markerElementComponent) where TGui : GuiComponent, new()
    {
        markerElementComponent.EntityEntered = (markerElementComponent, enteredPickup, entity) =>
        {
            if (!entity.HasComponent<GuiComponent>())
                entity.AddComponent(new TGui());
        };
        markerElementComponent.EntityLeft = (markerElementComponent, leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }

    public static void AddOpenGuiLogic<TGui>(this MarkerElementComponent markerElementComponent, Func<Entity, Task<TGui>> factory) where TGui : GuiComponent
    {
        markerElementComponent.EntityEntered = async (markerElementComponent, enteredPickup, entity) =>
        {
            if (!entity.HasComponent<GuiComponent>())
                entity.AddComponent(await factory(entity));
        };
        markerElementComponent.EntityLeft = (markerElementComponent, leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }

    public static void AddOpenGuiLogic<TGui1, TGui2>(this MarkerElementComponent markerElementComponent)
        where TGui1 : GuiComponent, new()
        where TGui2 : GuiComponent, new()
    {
        markerElementComponent.EntityEntered = (markerElementComponent, enteredPickup, entity) =>
        {
            if (!entity.HasComponent<GuiComponent>())
            {
                entity.AddComponent(new TGui1());
                entity.AddComponent(new TGui2());
            }
        };
        markerElementComponent.EntityLeft = (markerElementComponent, leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui1>())
                entity.DestroyComponent<TGui1>();
            if (entity.HasComponent<TGui2>())
                entity.DestroyComponent<TGui2>();
        };
    }

    public static void AddOpenGuiPageLogic<TGui>(this MarkerElementComponent markerElementComponent) where TGui : GuiPageComponent, new()
    {
        markerElementComponent.EntityEntered = (markerElementComponent, enteredPickup, entity) =>
        {
            if (!entity.HasComponent<GuiPageComponent>())
                entity.AddComponent(new TGui());
        };
        markerElementComponent.EntityLeft = (markerElementComponent, leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }

    public static void AddOpenGuiPageLogic<TGui>(this MarkerElementComponent markerElementComponent, Func<Entity, Task<TGui>> factory) where TGui : GuiPageComponent
    {
        markerElementComponent.EntityEntered = async (markerElementComponent, enteredPickup, entity) =>
        {
            if (!entity.HasComponent<GuiPageComponent>())
                entity.AddComponent(await factory(entity));
        };
        markerElementComponent.EntityLeft = (markerElementComponent, leftPickup, entity) =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }

}
