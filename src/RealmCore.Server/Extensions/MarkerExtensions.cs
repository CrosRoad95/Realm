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
                if(!player.HasComponent<GuiComponent>())
                    player.AddComponent(new TGui());
            }
        };
        marker.Left += (leftPickup, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<TGui>())
                    player.TryDestroyComponent<TGui>();
            }
        };
    }

    public static void AddOpenGuiLogic<TGui>(this RealmMarker marker, Func<RealmPlayer, Task<TGui>> factory) where TGui : GuiComponent
    {
        marker.Entered += async (enteredMarker, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<GuiComponent>())
                    player.AddComponent(await factory(player));
            }
        };
        marker.Left += (leftPickup, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (!player.HasComponent<TGui>())
                    player.TryDestroyComponent<TGui>();
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
                if (!player.HasComponent<GuiComponent>())
                {
                    player.AddComponent(new TGui1());
                    player.AddComponent(new TGui2());
                }
            }
        };

        marker.Left += (leftPickup, element) =>
        {
            if (element is RealmPlayer player)
            {
                if (player.HasComponent<TGui1>())
                    player.DestroyComponent<TGui1>();
                if (player.HasComponent<TGui2>())
                    player.DestroyComponent<TGui2>();
            }
        };
    }
}
