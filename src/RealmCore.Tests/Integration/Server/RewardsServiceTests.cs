namespace RealmCore.Tests.Integration.Server;

public class RewardsServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>
{
    private readonly RealmTestingServerHostingFixtureWithUniquePlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly RewardsService _rewardsService;

    public RewardsServiceTests(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _rewardsService = _fixture.Hosting.GetRequiredService<RewardsService>();
    }

    [Fact]
    public async Task RewardsShouldWork()
    {
        using var monitor = _rewardsService.Monitor();
        var added1 = await _rewardsService.TryGiveReward(_player, 1);
        var added2 = await _rewardsService.TryGiveReward(_player, 1);
        var rewards = await _rewardsService.GetRewards(_player);

        using var _ = new AssertionScope();
        added1.Should().BeTrue();
        added2.Should().BeFalse();
        rewards.Should().BeEquivalentTo([1]);
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["RewardGiven"]);
    }
}
