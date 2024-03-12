namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class NametagResourceLogic
{
    private readonly INametagsService _nametagsService;

    public NametagResourceLogic(IPlayersEventManager playersEventManager, INametagsService nametagsService)
    {
        playersEventManager.PlayerJoined += HandlePlayerJoined;
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
