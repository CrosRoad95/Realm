namespace RealmCore.Tests.Tests.Components;

public class LevelComponentTests
{
    private uint PopulateLevelsRegistry(LevelsRegistry levelsRegistry)
    {
        uint totalRequiredExperience = 0;
        for (int i = 0; i < 10; i++)
        {
            var requiredExperience = (uint)(10 * i + 10);
            levelsRegistry.Add((uint)i + 1, new LevelRegistryEntry(requiredExperience));
            totalRequiredExperience += requiredExperience;
        }
        return totalRequiredExperience;
    }

    [Fact]
    public void TestIfLevelsAreCountingCorrectly()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var levelsRegistry = realmTestingServer.GetRequiredService<LevelsRegistry>();
        var totalRequiredExperience = PopulateLevelsRegistry(levelsRegistry);
        var levelComponent = player.AddComponentWithDI<LevelComponent>();

        int addedLevels = 0;
        levelComponent.LevelChanged += (e, level, up) =>
        {
            addedLevels++;
        };

        levelComponent.GiveExperience(50);
        addedLevels.Should().Be(2);
        levelComponent.Experience.Should().Be(20);
        levelComponent.NextLevelRequiredExperience.Should().Be(30);

        levelComponent.GiveExperience(1000);
        addedLevels.Should().Be(10);
        levelComponent.Experience.Should().Be(1050 - totalRequiredExperience);
        levelComponent.NextLevelRequiredExperience.Should().Be(uint.MaxValue);
    }
}
