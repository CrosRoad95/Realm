using SlipeServer.Server.Enums;

namespace RealmCore.Tests.Tests;

public class RealmPlayerTests
{
    //[Fact]
    public async Task Test1()
    {
        var realmTestingServer = new RealmTestingServer();

        {
            var player = realmTestingServer.CreatePlayer();
            await realmTestingServer.SignInPlayer(player);

            for(int i = 0; i < 25; i++)
            {
                player.Events.Add(i % 5, $"test{i}");
            }
            player.TriggerDisconnected(QuitReason.Quit);
        }

        {
            var player = realmTestingServer.CreatePlayer();
            await realmTestingServer.SignInPlayer(player);
            var events = player.Events;
            var initialCount1 = events.Count();
            var fetched1 = await events.FetchMore();
            var fetched2 = await events.FetchMore();
            var initialCount2 = events.Count();
            ;
        }
    }
}
