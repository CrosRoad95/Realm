namespace Realm.Server.Extensions;

public static class ComponentsExtensions
{
    public static void AddOpenGuiLogic<TGui>(this PickupElementComponent pickupElementComponent) where TGui : GuiComponent, new()
    {
        pickupElementComponent.EntityEntered = entity =>
        {
            if (!entity.HasComponent<GuiComponent>())
                entity.AddComponent(new TGui());
        };
        pickupElementComponent.EntityLeft = entity =>
        {
            if (entity.HasComponent<TGui>())
                entity.DestroyComponent<TGui>();
        };
    }
}
