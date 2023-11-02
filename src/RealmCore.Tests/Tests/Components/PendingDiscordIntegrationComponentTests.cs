namespace RealmCore.Tests.Tests.Components;

public class PendingDiscordIntegrationComponentTests
{
    [Fact]
    public void DiscordVerificationCodeShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var pendingDiscordIntegration = player.AddComponentWithDI<PendingDiscordIntegrationComponent>();

        var code = pendingDiscordIntegration.GenerateAndGetDiscordConnectionCode();

        pendingDiscordIntegration.Verify(code).Should().BeTrue();
    }

    [Fact]
    public void DiscordVerificationCodeShouldExpireAfter2Minutes()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var pendingDiscordIntegration = player.AddComponentWithDI<PendingDiscordIntegrationComponent>();
        var code = pendingDiscordIntegration.GenerateAndGetDiscordConnectionCode();

        realmTestingServer.TestDateTimeProvider.AddOffset(TimeSpan.FromMinutes(3));

        pendingDiscordIntegration.Verify(code).Should().BeFalse();
    }
}
