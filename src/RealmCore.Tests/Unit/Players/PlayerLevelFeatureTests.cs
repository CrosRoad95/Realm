namespace RealmCore.Tests.Unit.Players;

public class PlayerLevelFeatureTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerLevelFeature _level;
    private readonly uint _totalRequiredExperience;

    public PlayerLevelFeatureTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _level = _player.Level;

        _totalRequiredExperience = Seed(_fixture.Hosting.GetRequiredService<LevelsCollection>());
    }

    private uint Seed(LevelsCollection levelsCollection, int step = 10)
    {
        if(levelsCollection.HasKey(1))
            return 0;

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
    public void TestIfLevelsAreCountingCorrectly()
    {
        int addedLevels = 0;
        var monitor = _level.Monitor();

        _level.Changed += (e, level, levelChange) =>
        {
            if(levelChange == LevelChange.Increase)
                addedLevels++;
        };

        _level.GiveExperience(50);

        using var _ = new AssertionScope();

        addedLevels.Should().Be(2);
        _level.Experience.Should().Be(20);
        _level.NextLevelRequiredExperience.Should().Be(30);
        monitor.OccurredEvents[0].Parameters.Should().BeEquivalentTo(new object[] { _level, 1, LevelChange.Increase });
        monitor.OccurredEvents[1].Parameters.Should().BeEquivalentTo(new object[] { _level, 2, LevelChange.Increase });
        monitor.OccurredEvents[2].Parameters.Should().BeEquivalentTo(new object[] { _level, 0, 20 });
        monitor.GetOccurredEvents().Should().BeEquivalentTo(["Changed", "Changed", "ExperienceChanged", "VersionIncreased"]);
        monitor.Clear();

        _level.GiveExperience(1000);

        addedLevels.Should().Be(10);
        _level.Experience.Should().Be(1050 - _totalRequiredExperience);
        _level.NextLevelRequiredExperience.Should().Be(uint.MaxValue);
    }

    [Fact]
    public async Task GivingExperienceShouldBeThreadSafe()
    {
        await ParallelHelpers.Run(() =>
        {
            _level.GiveExperience(1);
        });

        using var _ = new AssertionScope();
        _level.Experience.Should().Be(250);
        _level.Current.Should().Be(10);
    }

    public void Dispose()
    {
        _level.Clear();
    }
}
