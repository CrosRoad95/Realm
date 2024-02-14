namespace RealmCore.Tests.Tests;

public class RealmPlayerTests : RealmIntegrationTestingBase
{
    protected override string DatabaseName => "PlayerNotificationsTests";

    [Fact]
    public async Task SavingAndLoadingPlayerShouldWork()
    {
        var realmTestingServer = await CreateServerAsync();
        var now = realmTestingServer.TestDateTimeProvider.Now;
        {
            var player = await CreatePlayerAsync(false);
            player.Name = "foo";
            await realmTestingServer.SignInPlayer(player);

            player.Position = new Vector3(1, 2, 3);
            player.Money.Amount = 12.345m;
            player.DailyVisits.Update(now.AddHours(24));
            player.DailyVisits.VisitsInRow.Should().Be(2);
            player.Settings.Set(1, "test");
            player.Bans.Add(1);
            player.Upgrades.TryAdd(1);

            await realmTestingServer.GetRequiredService<ISaveService>().Save(player);
            player.TriggerDisconnected(QuitReason.Quit);

            player.Money.Amount.Should().Be(0);
        }

        realmTestingServer.TestDateTimeProvider.AddOffset(TimeSpan.FromDays(1));

        {
            var player = await CreatePlayerAsync(false);
            player.Name = "foo";
            await realmTestingServer.SignInPlayer(player);
            player.TrySpawnAtLastPosition();

            player.Position.Should().Be(new Vector3(1, 2, 3));
            player.Money.Amount.Should().Be(12.345m);
            player.DailyVisits.VisitsInRow.Should().Be(2);
            player.DailyVisits.LastVisit.Should().Be(realmTestingServer.TestDateTimeProvider.Now.Date);
            player.Settings.Get(1).Should().Be("test");
            player.Bans.IsBanned(1).Should().BeTrue();
            player.Upgrades.Has(1).Should().BeTrue();
        }
    }

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
