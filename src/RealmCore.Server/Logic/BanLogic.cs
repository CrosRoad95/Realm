namespace RealmCore.Server.Logic;

internal sealed class BanLogic
{
    private readonly ILogger<BanLogic> _logger;
    private readonly IBanService _banService;
    private readonly IActiveUsers _activeUsers;
    private readonly IUsersService _usersService;

    public BanLogic(ILogger<BanLogic> logger, IBanService banService, IActiveUsers activeUsers, IUsersService usersService)
    {
        _logger = logger;
        _banService = banService;
        _activeUsers = activeUsers;
        _usersService = usersService;
        _banService.Banned += HandleBanned;
        _banService.SerialUnbanned += HandleSerialUnbanned;
        _banService.UserUnbanned += HandleUserUnbanned;
    }

    private void HandleUserUnbanned(int userId, int banId, int? banType)
    {
        if(_activeUsers.TryGetPlayerByUserId(userId, out var player) && player != null)
        {
            if (player.TryGetComponent(out UserComponent userComponent))
            {
                userComponent.Bans.RemoveBan(banId);
            }
        }
    }

    private void HandleSerialUnbanned(string serial, int banId, int? banType)
    {

    }

    private void HandleBanned(BanDTO banDTO)
    {
        RealmPlayer? player = null;
        if (banDTO.UserId != null)
        {
            _activeUsers.TryGetPlayerByUserId(banDTO.UserId.Value, out player);
        }
        else if(banDTO.Serial != null)
        {
            _usersService.TryFindPlayerBySerial(banDTO.Serial, out player);
        }

        if(player != null)
        {
            if(player.TryGetComponent(out UserComponent userComponent))
            {
                userComponent.Bans.AddBan(banDTO);
            }
        }
    }
}
