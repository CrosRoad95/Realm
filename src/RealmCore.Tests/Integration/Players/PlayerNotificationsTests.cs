namespace RealmCore.Tests.Integration.Players;

public class PlayerNotificationsTests : RealmIntegrationTestingBase
{
    protected override string DatabaseName => "PlayerNotificationsTests";

    [Fact]
    public async Task Test1()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var playerNotifications = server.GetRequiredService<IPlayersNotifications>();

        var playerNotificationsFeatureMonitor = player.Notifications.Monitor();
        var playerNotificationsMonitor = playerNotifications.Monitor();

        await playerNotifications.Create(player, "test title", "test desc", "excerpt", CancellationToken.None);

        player.Notifications.Count().Should().Be(1);
        var notification = player.Notifications.First();

        var now = server.DateTimeProvider.Now;

        notification.Should().BeEquivalentTo(new UserNotificationDto
        {
            Id = 1,
            Title = "test title",
            Content = "test desc",
            Excerpt = "excerpt",
            UserId = player.PersistentId,
            ReadTime = null,
            SentTime = server.DateTimeProvider.Now
        });

        var unread = await playerNotifications.CountUnread(player, CancellationToken.None);
        unread.Should().Be(1);

        server.DateTimeProvider.AddOffset(TimeSpan.FromMinutes(1));
        var now2 = server.DateTimeProvider.Now;

        var markedAsRead = await playerNotifications.TryMarkAsRead(notification.Id);
        notification = player.Notifications.First();
        markedAsRead.Should().BeTrue();
        notification.ReadTime.Should().Be(now2);

        unread = await playerNotifications.CountUnread(player, CancellationToken.None);
        unread.Should().Be(0);
    }
}
