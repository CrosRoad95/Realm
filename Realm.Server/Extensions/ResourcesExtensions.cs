namespace Realm.Server.Extensions;

internal static class ResourcesExtensions
{
    public static async Task StartForAsync(this Resource resource, Player player)
    {
        var wait = new TaskCompletionSource();

        ElementEventHandler<Player, PlayerResourceStartedEventArgs> resourceStarted = (player, eventArgs) => {
            if (eventArgs.NetId == resource.NetId)
                wait.SetResult();
        };

        player.ResourceStarted += resourceStarted;
        resource.StartFor(player);
        await wait.Task;
        player.ResourceStarted -= resourceStarted;
    }

    public static void AddGlobals(this Resource resource)
    {
        var fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Realm.Server.Resources.Globals.lua");
        resource.NoClientScripts[$"{resource!.Name}/Globals.lua"] = fileStream.ToByteArray();
    }
}
