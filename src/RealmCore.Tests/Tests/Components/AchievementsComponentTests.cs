namespace RealmCore.Tests.Tests.Components;

public class AchievementsComponentTests
{
    [Fact]
    public void TestIfAchievementProgressCountsCorrectly()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var achievementsComponent = player.AddComponent<AchievementsComponent>();
        var progressedTimes = 0;
        var progressedAchievementId = -1;
        var progressedAchievement = -1.0f;
        achievementsComponent.AchievementProgressed += (component, achievementId, progress) =>
        {
            progressedAchievementId = achievementId;
            progressedAchievement = progress;
            progressedTimes++;
        };
        achievementsComponent.UpdateProgress(1, 5, 100);
        achievementsComponent.UpdateProgress(1, 15, 100);
        achievementsComponent.GetProgress(1).Should().Be(20);
        progressedTimes.Should().Be(2);
        progressedAchievementId.Should().Be(1);
        progressedAchievement.Should().Be(20);
    }

    [Fact]
    public void TestIfCanReceiveReward()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var achievementsComponent = player.AddComponent<AchievementsComponent>();

        var unlockedAchievement = -1;
        achievementsComponent.AchievementUnlocked += (component, achievementId) =>
        {
            component.Should().Be(achievementsComponent);
            unlockedAchievement = achievementId;
        };

        achievementsComponent.TryReceiveReward(2, 100).Should().BeFalse();

        achievementsComponent.UpdateProgress(2, 100, 100);
        achievementsComponent.TryReceiveReward(2, 100).Should().BeTrue();
        achievementsComponent.TryReceiveReward(2, 100).Should().BeFalse();
        unlockedAchievement.Should().Be(2);
    }
}
