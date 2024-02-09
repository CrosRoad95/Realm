namespace RealmCore.Tests.Tests.PlayerServices;

public class PlayerLevelServiceTests
{
    private uint PopulateLevelsCollection(LevelsCollection levelsCollection)
    {
        uint totalRequiredExperience = 0;
        for (int i = 0; i < 10; i++)
        {
            var requiredExperience = (uint)(10 * i + 10);
            levelsCollection.Add((uint)i + 1, new LevelsCollectionItem(requiredExperience));
            totalRequiredExperience += requiredExperience;
        }
        return totalRequiredExperience;
    }

    [Fact]
    public void TestIfLevelsAreCountingCorrectly()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var levelsCollection = realmTestingServer.GetRequiredService<LevelsCollection>();
        var totalRequiredExperience = PopulateLevelsCollection(levelsCollection);
        var level = player.Level;

        int addedLevels = 0;
        level.LevelChanged += (e, level, up) =>
        {
            addedLevels++;
        };

        level.GiveExperience(50);
        addedLevels.Should().Be(2);
        level.Experience.Should().Be(20);
        level.NextLevelRequiredExperience.Should().Be(30);

        level.GiveExperience(1000);
        addedLevels.Should().Be(10);
        level.Experience.Should().Be(1050 - totalRequiredExperience);
        level.NextLevelRequiredExperience.Should().Be(uint.MaxValue);
    }
}
