namespace RealmCore.Tests.Repositories;

public class UserNotificationRepositoryTests
{
    private readonly RealmTestingServer _server;
    public UserNotificationRepositoryTests()
    {
        _server = new();
    }

    [Fact]
    public async Task ItShouldBePossibleToRead()
    {
        var dateTimeProvider = _server.GetRequiredService<IDateTimeProvider>();
        var userNotificationRepository = _server.GetRequiredService<IUserNotificationRepository>();

        var notification = await userNotificationRepository.Create(2, dateTimeProvider.Now, "test", "test content");
        var unreadNotificationBefore = await userNotificationRepository.CountUnread(2);
        var markAsReadResult1 = await userNotificationRepository.MarkAsRead(notification.Id, dateTimeProvider.Now);
        var markAsReadResult2 = await userNotificationRepository.MarkAsRead(notification.Id, dateTimeProvider.Now);
        var unreadNotificationAfter = await userNotificationRepository.CountUnread(2);

        unreadNotificationBefore.Should().Be(1);
        unreadNotificationAfter.Should().Be(0);
        markAsReadResult1.Should().BeTrue();
        markAsReadResult2.Should().BeFalse();
    }
}
