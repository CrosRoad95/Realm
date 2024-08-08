namespace RealmCore.Server.Modules.Players.Groups;

internal sealed class GroupsLogic : PlayerLifecycle, IHostedService
{
    private readonly GroupsManager _groupsManager;

    public GroupsLogic(PlayersEventManager playersEventManager, GroupsManager groupsManager) : base(playersEventManager)
    {
        _groupsManager = groupsManager;
    }

    protected override Task PlayerLoggedIn(PlayerUserFeature user, RealmPlayer player)
    {
        foreach (var group in player.Groups)
        {
            _groupsManager.AddPlayerToGroup(group.GroupId, player);
        }
        return Task.CompletedTask;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        _groupsManager.RemovePlayerFromAllGroups(player);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
