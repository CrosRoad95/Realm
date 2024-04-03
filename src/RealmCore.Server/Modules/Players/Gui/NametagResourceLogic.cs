namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class NametagResourceLogic
{
    private readonly INametagsService _nametagsService;

    public NametagResourceLogic(PlayersEventManager playersEventManager, INametagsService nametagsService)
    {
        playersEventManager.Joined += HandlePlayerJoined;
        _nametagsService = nametagsService;
    }

    private void HandlePlayerJoined(Player plr)
    {
        if(plr is RealmPlayer player)
            player.NametagTextChanged += HandleNametagTextChanged;
    }

    private void HandleNametagTextChanged(RealmPlayer player, string? nametagText)
    {
        if (nametagText != null)
        {
            _nametagsService.SetNametag(player, nametagText);
        }
        else
        {
            _nametagsService.RemoveNametag(player);
        }
    }
}
