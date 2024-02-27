namespace RealmCore.Tests.Tests;

public class RealmPlayerTests : RealmIntegrationTestingBase
{
    protected override string DatabaseName => "RealmPlayerTests";

    [Fact]
    public async Task SavingAndLoadingPlayerShouldWork()
    {
        var server = await CreateServerAsync();
        var now = server.DateTimeProvider.Now;
        {
            var player = await CreatePlayerAsync(false);
            player.Name = "foo";
            await server.SignInPlayer(player);
            player.Spawn(new Vector3(1, 2, 3));

            player.PlayTime.InternalSetTotalPlayTime(1337);
            player.Money.Amount = 12.345m;
            player.DailyVisits.Update(now.AddHours(24));
            player.DailyVisits.VisitsInRow.Should().Be(1);
            player.Settings.Set(1, "test");
            player.Bans.Add(1, reason: "test");
            player.Upgrades.TryAdd(1);
            player.Statistics.Increase(1, 1);
            player.Statistics.Increase(2, 2);

            await player.GetRequiredService<IUsersService>().SignOut(player);
            player.TriggerDisconnected(QuitReason.Quit);

            player.Money.Amount.Should().Be(0);
        }

        server.DateTimeProvider.AddOffset(TimeSpan.FromDays(1));

        void assert(RealmTestingServer server, RealmPlayer player)
        {
            player.Position.Should().Be(new Vector3(1, 2, 3));
            player.PlayTime.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1337));
            player.Money.Amount.Should().Be(12.345m);
            //player.DailyVisits.VisitsInRow.Should().Be(2); // TODO:
            player.DailyVisits.LastVisit.Date.Should().Be(server.DateTimeProvider.Now.Date);
            player.Settings.Get(1).Should().Be("test");
            player.Bans.IsBanned(1).Should().BeTrue();
            player.Upgrades.Has(1).Should().BeTrue();
            player.Statistics.Should().BeEquivalentTo([new UserStatDto(1, 1), new UserStatDto(2, 2)]);
        }

        for(int i = 0; i < 2; i++)
        {
            var player = await CreatePlayerAsync(false);
            player.Name = "foo";
            await server.SignInPlayer(player);
            player.TrySpawnAtLastPosition().Should().BeTrue();

            assert(server, player);

            await player.GetRequiredService<IUsersService>().SignOut(player);
            player.TriggerDisconnected(QuitReason.Quit);
        }
    }

    //[Fact]
    public async Task Test1()
    {
        var server = new RealmTestingServer();

        {
            var player = server.CreatePlayer();
            await server.SignInPlayer(player);

            for(int i = 0; i < 25; i++)
            {
                player.Events.Add(i % 5, $"test{i}");
            }
            player.TriggerDisconnected(QuitReason.Quit);
        }

        {
            var player = server.CreatePlayer();
            await server.SignInPlayer(player);
            var events = player.Events;
            var initialCount1 = events.Count();
            var fetched1 = await events.FetchMore();
            var fetched2 = await events.FetchMore();
            var initialCount2 = events.Count();
            ;
        }
    }
}
