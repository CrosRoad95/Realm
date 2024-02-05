namespace RealmCore.Server.Logic.Resources;

internal sealed class NametagResourceLogic
{
    private readonly INametagsService _nametagsService;

    public NametagResourceLogic(MtaServer mtaServer, INametagsService nametagsService)
    {
        mtaServer.PlayerJoined += HandlePlayerJoined;
        _nametagsService = nametagsService;
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (RealmPlayer)plr;
        player.NametagTextChanged += HandleNametagTextChanged;
    }

    private void HandleNametagTextChanged(RealmPlayer player, string? nametagText)
    {
        if(nametagText != null)
        {
            _nametagsService.SetNametag((Ped)player, nametagText);
        }
        else
        {
            _nametagsService.RemoveNametag((Ped)player);
        }
    }
}
