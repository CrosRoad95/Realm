namespace RealmCore.Tests.Unit.Players;

public class PlayerLevelFeatureTests
{
    private uint Seed(RealmTestingServerHosting hosting, int step = 10)
    {
        var levelsCollection = hosting.GetRequiredService<LevelsCollection>();
        uint totalRequiredExperience = 0;
        for (int i = 0; i < 10; i++)
        {
            var requiredExperience = (uint)(step * i + step);
            levelsCollection.Add((uint)i + 1, new LevelsCollectionItem(requiredExperience));
            totalRequiredExperience += requiredExperience;
        }
        return totalRequiredExperience;
    }

    [Fact]
    public async Task TestIfLevelsAreCountingCorrectly()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var totalRequiredExperience = Seed(hosting);
        var level = player.Level;

        int addedLevels = 0;
        var monitor = level.Monitor();

        level.LevelChanged += (e, level, levelChange) =>
        {
            if(levelChange == LevelChange.Increase)
                addedLevels++;
        };

        level.GiveExperience(50);

        addedLevels.Should().Be(2);
        level.Experience.Should().Be(20);
        level.NextLevelRequiredExperience.Should().Be(30);
        monitor.OccurredEvents[0].Parameters.Should().BeEquivalentTo(new object[] { level, 1, LevelChange.Increase });
        monitor.OccurredEvents[1].Parameters.Should().BeEquivalentTo(new object[] { level, 2, LevelChange.Increase });
        monitor.OccurredEvents[2].Parameters.Should().BeEquivalentTo(new object[] { level, 0, 20 });
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["LevelChanged", "LevelChanged", "ExperienceChanged"]);
        monitor.Clear();

        level.GiveExperience(1000);
        addedLevels.Should().Be(10);
        level.Experience.Should().Be(1050 - totalRequiredExperience);
        level.NextLevelRequiredExperience.Should().Be(uint.MaxValue);

        level.Current = 0;
        level.Experience = 0;
    }

    [Fact]
    public async Task GivingExperienceShouldBeThreadSafe()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var totalRequiredExperience = Seed(hosting, 10000);
        var level = player.Level;

        await ParallelHelpers.Run(() =>
        {
            level.GiveExperience(1);
        });

        level.Experience.Should().Be(800);
    }
}
