using RealmCore.Persistence.Data;

namespace RealmCore.Tests.Repositories;

public class UserNotificationRepositoryTests
{
    private readonly RealmTestingServer _server;
    public UserNotificationRepositoryTests()
    {
        _server = new();
    }

    [Fact]
    public async Task ListingNotificationsShouldWork()
    {
        var dateTimeProvider = _server.GetRequiredService<IDateTimeProvider>();
        var userNotificationRepository = _server.GetRequiredService<IUserNotificationRepository>();

        for(int i = 0 ; i < 20; i++)
        {
            await userNotificationRepository.Create(1, dateTimeProvider.Now.AddSeconds(i), "test", "test content");
        }

        var notifications = await userNotificationRepository.Get(1);
        notifications.First().Should().BeEquivalentTo(new UserNotificationData
        {
            Id = 20,
            UserId = 1,
            Title = "test",
            Content = "test content",
            Excerpt = "test content",
            SentTime = dateTimeProvider.Now.AddSeconds(19),
            ReadTime = null,
        });
        notifications.Last().Should().BeEquivalentTo(new UserNotificationData
        {
            Id = 11,
            UserId = 1,
            Title = "test",
            Content = "test content",
            Excerpt = "test content",
            SentTime = dateTimeProvider.Now.AddSeconds(10),
            ReadTime = null,
        });
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
