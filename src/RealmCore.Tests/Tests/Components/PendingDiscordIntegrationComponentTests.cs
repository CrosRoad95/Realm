using RealmCore.ECS;

namespace RealmCore.Tests.Tests.Components;

public class PendingDiscordIntegrationComponentTests
{
    private readonly Entity _entity1;
    private readonly PendingDiscordIntegrationComponent _pendingDiscordIntegration;
    private readonly TestDateTimeProvider _testDateTimeProvider;

    public PendingDiscordIntegrationComponentTests()
    {
        _testDateTimeProvider = new();
        var services = new ServiceCollection();
        services.AddSingleton<IDateTimeProvider>(_testDateTimeProvider);

        var serviceProvider = services.BuildServiceProvider();
        _entity1 = new("test");
        _pendingDiscordIntegration = new(_testDateTimeProvider);
        _entity1.AddComponent(_pendingDiscordIntegration);
    }

    [Fact]
    public void DiscordVerificationCodeShouldWork()
    {
        var code = _pendingDiscordIntegration.GenerateAndGetDiscordConnectionCode();

        _pendingDiscordIntegration.Verify(code).Should().BeTrue();
    }

    [Fact]
    public void DiscordVerificationCodeShouldExpireAfter2Minutes()
    {
        var code = _pendingDiscordIntegration.GenerateAndGetDiscordConnectionCode();

        _testDateTimeProvider.AddOffset(TimeSpan.FromMinutes(3));

        _pendingDiscordIntegration.Verify(code).Should().BeFalse();
    }
}
