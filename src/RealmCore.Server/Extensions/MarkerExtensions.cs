using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Extensions;

public static class MarkerExtensions
{
    public static void AddOpenGuiLogic<TGui>(this RealmMarker marker) where TGui : GuiComponent, new()
    {
        marker.Entered += (enteredMarker, element) =>
        {
            if(element is RealmPlayer player)
            {
                if(!player.Components.HasComponent<GuiComponent>())
                    player.Components.AddComponent(new TGui());
            }
        };
        marker.Left += (leftPickup, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.Components.HasComponent<TGui>())
                    player.Components.TryDestroyComponent<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this RealmMarker marker, Func<RealmPlayer, Task<TGui>> factory) where TGui : GuiComponent
    {
        marker.Entered += async (enteredMarker, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.Components.HasComponent<GuiComponent>())
                    player.Components.AddComponent(await factory(player));
            }
        };
        marker.Left += (leftPickup, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.Components.HasComponent<TGui>())
                    player.Components.TryDestroyComponent<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui1, TGui2>(this RealmMarker marker)
        where TGui1 : GuiComponent, new()
        where TGui2 : GuiComponent, new()
    {
        marker.Entered += (enteredPickup, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.Components.HasComponent<GuiComponent>())
                {
                    player.Components.AddComponent(new TGui1());
                    player.Components.AddComponent(new TGui2());
                }
            }
        };

        marker.Left += (leftPickup, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (player.Components.HasComponent<TGui1>())
                    player.Components.DestroyComponent<TGui1>();
                if (player.Components.HasComponent<TGui2>())
                    player.Components.DestroyComponent<TGui2>();
            }
        };
    }
}
