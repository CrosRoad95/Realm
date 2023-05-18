namespace RealmCore.Tests.Tests.Components;

public class LevelComponentTests
{
    private readonly Entity _entity;
    private readonly LevelComponent _levelComponent;
    private readonly uint _totalRequiredExperience = 0;

    public LevelComponentTests()
    {
        var services = new ServiceCollection();
        services.AddSingleton<LevelsRegistry>();

        var serviceProvider = services.BuildServiceProvider();
        var levelsRegistry = serviceProvider.GetRequiredService<LevelsRegistry>();
        for(int i = 0; i < 10;i++)
        {
            var requiredExperience = (uint)(10 * i + 10);
            levelsRegistry.Add((uint)i + 1, new LevelRegistryEntry(requiredExperience));
            _totalRequiredExperience += requiredExperience;
        }
        _entity = new(serviceProvider, "test", EntityTag.Unknown);
        _levelComponent = new();
        _entity.AddComponent(_levelComponent);
    }

    [Fact]
    public void TestIfLevelsAreCountingCorrectly()
    {
        int addedLevels = 0;
        _levelComponent.LevelChanged += (e, level, up) =>
        {
            addedLevels++;
        };

        _levelComponent.GiveExperience(50);
        addedLevels.Should().Be(2);
        _levelComponent.Experience.Should().Be(20);
        _levelComponent.NextLevelRequiredExperience.Should().Be(30);

        _levelComponent.GiveExperience(1000);
        addedLevels.Should().Be(10);
        _levelComponent.Experience.Should().Be(1050 - _totalRequiredExperience);
        _levelComponent.NextLevelRequiredExperience.Should().Be(uint.MaxValue);
    }
}
