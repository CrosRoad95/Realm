namespace RealmCore.Tests.Unit.Players;

public class PlayerAchievementsFeatureTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerAchievementsFeature _achievements;
    private readonly TestDateTimeProvider _dateTimeProvider;

    public PlayerAchievementsFeatureTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _achievements = _player.Achievements;
        _dateTimeProvider = _fixture.Hosting.DateTimeProvider;
    }

    [Fact]
    public void TestIfAchievementProgressCountsCorrectly()
    {
        var progressedTimes = 0;
        var progressedAchievementId = -1;
        var progressedAchievement = -1.0f;
        _achievements.Progressed += (that, achievementId, progress) =>
        {
            progressedAchievementId = achievementId;
            progressedAchievement = progress;
            progressedTimes++;
        };
        _achievements.UpdateProgress(1, 5, 100);
        _achievements.UpdateProgress(1, 15, 100);
        _achievements.GetProgress(1).Should().Be(20);
        progressedTimes.Should().Be(2);
        progressedAchievementId.Should().Be(1);
        progressedAchievement.Should().Be(20);
    }

    [Fact]
    public void TestIfCanReceiveReward()
    {
        var unlockedAchievement = -1;
        _achievements.Unlocked += (that, achievementId) =>
        {
            unlockedAchievement = achievementId;
        };

        var now = _dateTimeProvider.Now;
        _achievements.TryReceiveReward(2, 100, now).Should().BeFalse();

        _achievements.UpdateProgress(2, 100, 100);
        _achievements.TryReceiveReward(2, 100, now).Should().BeTrue();
        _achievements.TryReceiveReward(2, 100, now).Should().BeFalse();
        unlockedAchievement.Should().Be(2);
    }

    public void Dispose()
    {
        _achievements.Clear();
    }

}
