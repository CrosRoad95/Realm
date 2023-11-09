using RealmCore.Resources.Overlay;
using RealmCore.Server.Interfaces.Players;
using SlipeServer.Server;

namespace RealmCore.Sample.Logic;

internal sealed class AchievementsLogic
{
    private readonly IOverlayService _overlayService;

    public AchievementsLogic(MtaServer mtaServer)
    {
        mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player plr)
    {
        var player = (RealmPlayer)plr;
        player.Achievements.Unlocked += HandleUnlocked;
    }

    private void HandleUnlocked(IPlayerAchievementsService achievementService, int achievementId)
    {
        _overlayService.AddNotification(achievementService.Player, $"Odblokowałeś osiągnięcie '{achievementId}'");
    }
}
