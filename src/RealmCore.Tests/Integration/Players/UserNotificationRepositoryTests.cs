namespace RealmCore.Tests.Integration.Players;

[Collection("IntegrationTests")]
public class UserNotificationRepositoryTests : RealmRemoteDatabaseIntegrationTestingBase
{
    [Fact]
    public async Task ItShouldBePossibleToRead()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();
        var userId = player.UserId;

        var dateTimeProvider = server.GetRequiredService<IDateTimeProvider>();
        var userNotificationRepository = server.GetRequiredService<IUserNotificationRepository>();

        var notification = await userNotificationRepository.Create(userId, dateTimeProvider.Now, "test", "test content");
        var unreadNotificationBefore = await userNotificationRepository.CountUnread(userId);
        var markAsReadResult1 = await userNotificationRepository.MarkAsRead(notification.Id, dateTimeProvider.Now);
        var markAsReadResult2 = await userNotificationRepository.MarkAsRead(notification.Id, dateTimeProvider.Now);
        var unreadNotificationAfter = await userNotificationRepository.CountUnread(userId);

        unreadNotificationBefore.Should().Be(1);
        unreadNotificationAfter.Should().Be(0);
        markAsReadResult1.Should().BeTrue();
        markAsReadResult2.Should().BeFalse();
    }
}
