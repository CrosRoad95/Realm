namespace RealmCore.Server.Interfaces;

public interface IFocusableElement
{
    int FocusedPlayerCount { get; }
    IEnumerable<RealmPlayer> FocusedPlayers { get; }

    event Action<Element, RealmPlayer>? PlayerFocused;
    event Action<Element, RealmPlayer>? PlayerLostFocus;

    internal bool AddFocusedPlayer(RealmPlayer player);
    internal bool RemoveFocusedPlayer(RealmPlayer player);
}
