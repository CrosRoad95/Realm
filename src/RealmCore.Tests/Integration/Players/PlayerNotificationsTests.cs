namespace RealmCore.Tests.Integration.Players;

[Collection("IntegrationTests")]
public class PlayerNotificationsTests : RealmRemoteDatabaseIntegrationTestingBase
{
    [Fact]
    public async Task ItShouldBePossibleToReadNotification()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var userId = player.UserId;
        var sut = player.Notifications;
        var playersNotifications = hosting.Host.Services.GetRequiredService<IPlayersNotifications>();

        var notificationDto = await playersNotifications.Create(userId, "test title", "test desc", "excerpt content");

        sut.Should().BeEquivalentTo([notificationDto]);

        notificationDto.IsRead.Should().BeFalse();

        await playersNotifications.TryMarkAsRead(notificationDto.Id);

        sut.First().IsRead.Should().BeTrue();

        //var server = await CreateServerAsync();
        //    var player = await CreatePlayerAsync();
        //    var userId = player.UserId;

        //    var dateTimeProvider = server.GetRequiredService<IDateTimeProvider>();
        //    var userNotificationRepository = server.GetRequiredService<IUserNotificationRepository>();

        //    var notification = await userNotificationRepository.Create(userId, dateTimeProvider.Now, "test", "test content");
        //    var unreadNotificationBefore = await userNotificationRepository.CountUnread(userId);
        //    var markAsReadResult1 = await userNotificationRepository.MarkAsRead(notification.Id, dateTimeProvider.Now);
        //    var markAsReadResult2 = await userNotificationRepository.MarkAsRead(notification.Id, dateTimeProvider.Now);
        //    var unreadNotificationAfter = await userNotificationRepository.CountUnread(userId);

        //    unreadNotificationBefore.Should().Be(1);
        //    unreadNotificationAfter.Should().Be(0);
        //    markAsReadResult1.Should().BeTrue();
        //    markAsReadResult2.Should().BeFalse();
    }

    //[Fact]
    //public async Task Test1()
    //{
    //    var server = await CreateServerAsync();
    //    var player = await CreatePlayerAsync();

    //    var playerNotifications = server.GetRequiredService<IPlayersNotifications>();

    //    var playerNotificationsFeatureMonitor = player.Notifications.Monitor();
    //    var playerNotificationsMonitor = playerNotifications.Monitor();

    //    var createdNotificationDto = await playerNotifications.Create(player, "test title", "test desc", "excerpt", CancellationToken.None);

    //    player.Notifications.Count().Should().Be(1);
    //    var notification = player.Notifications.First();

    //    notification.Should().BeEquivalentTo(new UserNotificationDto
    //    {
    //        Id = createdNotificationDto.Id,
    //        Title = "test title",
    //        Content = "test desc",
    //        Excerpt = "excerpt",
    //        UserId = player.UserId,
    //        ReadTime = null,
    //        SentTime = server.DateTimeProvider.Now
    //    });

    //    var unread = await playerNotifications.CountUnread(player, CancellationToken.None);
    //    unread.Should().Be(1);

    //    server.DateTimeProvider.AddOffset(TimeSpan.FromMinutes(1));
    //    var now2 = server.DateTimeProvider.Now;

    //    var markedAsRead = await playerNotifications.TryMarkAsRead(notification.Id);
    //    notification = player.Notifications.First();
    //    markedAsRead.Should().BeTrue();
    //    notification.ReadTime.Should().Be(now2);

    //    unread = await playerNotifications.CountUnread(player, CancellationToken.None);
    //    unread.Should().Be(0);
    //}
}
