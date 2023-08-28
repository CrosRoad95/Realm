using RealmCore.ECS;

namespace RealmCore.Tests.Tests.Components;

public class AchievementsComponentTests
{
    private readonly Entity _entity;
    private readonly AchievementsComponent _achievementsComponent;

    public AchievementsComponentTests()
    {
        _entity = new("test");
        _achievementsComponent = new();
        _entity.AddComponent(_achievementsComponent);
    }

    [Fact]
    public void TestIfAchievementProgressCountsCorrectly()
    {
        var progressedTimes = 0;
        var progressedAchievementId = -1;
        var progressedAchievement = -1.0f;
        _achievementsComponent.AchievementProgressed += (component, achievementId, progress) =>
        {
            progressedAchievementId = achievementId;
            progressedAchievement = progress;
            progressedTimes++;
        };
        _achievementsComponent.UpdateProgress(1, 5, 100);
        _achievementsComponent.UpdateProgress(1, 15, 100);
        _achievementsComponent.GetProgress(1).Should().Be(20);
        progressedTimes.Should().Be(2);
        progressedAchievementId.Should().Be(1);
        progressedAchievement.Should().Be(20);
    }

    [Fact]
    public void TestIfCanReceiveReward()
    {
        var unlockedAchievement = -1;
        _achievementsComponent.AchievementUnlocked += (component, achievementId) =>
        {
            component.Should().Be(_achievementsComponent);
            unlockedAchievement = achievementId;
        };

        _achievementsComponent.TryReceiveReward(2, 100).Should().BeFalse();

        _achievementsComponent.UpdateProgress(2, 100, 100);
        _achievementsComponent.TryReceiveReward(2, 100).Should().BeTrue();
        _achievementsComponent.TryReceiveReward(2, 100).Should().BeFalse();
        unlockedAchievement.Should().Be(2);
    }
}
