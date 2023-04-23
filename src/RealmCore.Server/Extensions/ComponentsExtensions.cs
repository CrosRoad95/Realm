namespace RealmCore.Server.Extensions;

public static class ComponentsExtensions
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
}
