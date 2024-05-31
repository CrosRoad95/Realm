namespace RealmCore.Tests.Unit.Players;

public class PlayerAchievementsServiceTests
{
    [Fact]
    public async Task TestIfAchievementProgressCountsCorrectly()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var achievements = player.Achievements;
        var progressedTimes = 0;
        var progressedAchievementId = -1;
        var progressedAchievement = -1.0f;
        achievements.Progressed += (that, achievementId, progress) =>
        {
            progressedAchievementId = achievementId;
            progressedAchievement = progress;
            progressedTimes++;
        };
        achievements.UpdateProgress(1, 5, 100);
        achievements.UpdateProgress(1, 15, 100);
        achievements.GetProgress(1).Should().Be(20);
        progressedTimes.Should().Be(2);
        progressedAchievementId.Should().Be(1);
        progressedAchievement.Should().Be(20);
    }

    [Fact]
    public async Task TestIfCanReceiveReward()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var achievements = player.Achievements;

        var unlockedAchievement = -1;
        achievements.Unlocked += (that, achievementId) =>
        {
            unlockedAchievement = achievementId;
        };

        var now = hosting.DateTimeProvider.Now;
        achievements.TryReceiveReward(2, 100, now).Should().BeFalse();

        achievements.UpdateProgress(2, 100, 100);
        achievements.TryReceiveReward(2, 100, now).Should().BeTrue();
        achievements.TryReceiveReward(2, 100, now).Should().BeFalse();
        unlockedAchievement.Should().Be(2);
    }
}
