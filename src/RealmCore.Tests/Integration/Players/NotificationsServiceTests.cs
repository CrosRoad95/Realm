namespace RealmCore.Tests.Integration.Players;

public class NotificationsServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>
{
    private readonly RealmTestingServerHostingFixtureWithUniquePlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly NotificationsService _notificationsService;

    public NotificationsServiceTests(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _notificationsService = _fixture.Hosting.GetRequiredService<NotificationsService>();
    }

    [Fact]
    public async Task NotificationsShouldWork()
    {
        using var monitor = _notificationsService.Monitor();
        var now = _fixture.Hosting.DateTimeProvider.Now;
        var notification = await _notificationsService.Create(_player.UserId, "title", "desc", "excerpt");
        var notifications = await _notificationsService.Get(_player.UserId);
        var notificationById = await _notificationsService.GetById(notification.Id);

        using var _ = new AssertionScope();

        notification.Should().BeEquivalentTo(new UserNotificationDto
        {
            Id = notification.Id,
            UserId = _player.UserId,
            Content = "desc",
            Excerpt = "excerpt",
            ReadTime = null,
            SentTime = now,
            Title = "title",
        }, options =>
            options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(500))).WhenTypeIs<DateTime>());

        notifications.Should().BeEquivalentTo([notification], options =>
            options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(500))).WhenTypeIs<DateTime>());
        notificationById.Should().BeEquivalentTo(notification, options =>
            options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(500))).WhenTypeIs<DateTime>());
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["Created"]);
    }
}
