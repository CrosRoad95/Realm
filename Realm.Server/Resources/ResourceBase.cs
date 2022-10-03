using SlipeServer.Server.Elements.Events;

namespace Realm.Server.Resources;

public class ResourceBase : Resource
{
    public ResourceBase(MtaServer server, RootElement root, string name, string? path = null) : base(server, root, name, path)
    {
    }

    public async Task StartForAsync(Player player)
    {
        var wait = new TaskCompletionSource();
        ElementEventHandler<Player, PlayerResourceStartedEventArgs> resourceStarted = (player, eventArgs) => {
            if (eventArgs.NetId == NetId)
                wait.SetResult();
        };

        player.ResourceStarted += resourceStarted;
        StartFor(player);
        await wait.Task;
        player.ResourceStarted -= resourceStarted;
    }
}
