namespace RealmCore.Tests.Unit.Players;

public class PlayerDailyTasksFeatureTests
{
    private readonly PlayerDailyTasksFeature _sut;
    private readonly TestDateTimeProvider _testDateTimeProvider = new();

    public PlayerDailyTasksFeatureTests()
    {

        var testPlayerContext = new PlayerContext
        {
            Player = new()
        };

        _sut = new PlayerDailyTasksFeature(testPlayerContext, _testDateTimeProvider);
    }

    [Fact]
    public void YouCanStartOnlyOneDailyTaskOfGivenTypePerDay()
    {
        _sut.TryBeginDailyTask(1).Should().BeTrue();
        _sut.TryBeginDailyTask(1).Should().BeFalse();
        _sut.TryBeginDailyTask(2).Should().BeTrue();
        _sut.TryBeginDailyTask(2).Should().BeFalse();

        _testDateTimeProvider.Add(TimeSpan.FromDays(1));
        _sut.TryBeginDailyTask(1).Should().BeTrue();
        _sut.TryBeginDailyTask(1).Should().BeFalse();
        _sut.TryBeginDailyTask(2).Should().BeTrue();
        _sut.TryBeginDailyTask(2).Should().BeFalse();
    }

    [Fact]
    public void YouShouldBeAbleToGetTask()
    {
        _sut.TryBeginDailyTask(1);
        _sut.GetTask(1).Should().BeEquivalentTo(new DailyTaskProgressDto
        {
            Id = 0,
            CreatedAt = _testDateTimeProvider.Now,
            DailyTaskId = 1,
            Progress = 0
        });
        _sut.GetTask(1, _testDateTimeProvider.Now.AddDays(1)).Should().BeNull();
        _sut.GetTask(2).Should().BeNull();
    }

    [Fact]
    public void YouShouldBeAbleToDoProgress()
    {
        _sut.TryBeginDailyTask(1);
        _sut.TryDoProgress(1, 10).Should().BeTrue();
        _sut.TryDoProgress(1, 10).Should().BeTrue();
        _sut.GetProgress(1337).Should().Be(0);
        _sut.GetProgress(1).Should().Be(20);
        _sut.GetTask(1).Should().BeEquivalentTo(new DailyTaskProgressDto
        {
            Id = 0,
            CreatedAt = _testDateTimeProvider.Now,
            DailyTaskId = 1,
            Progress = 20
        });
    }

    [Fact]
    public void ProgressShouldBeMadeOnlyToCurrentTask()
    {
        _sut.TryBeginDailyTask(1);
        _testDateTimeProvider.Add(TimeSpan.FromDays(1));
        _sut.TryBeginDailyTask(1);
        _sut.TryDoProgress(1, 10);

        using var _ = new AssertionScope();
        _sut.GetProgress(1).Should().Be(10);
        _sut.GetProgress(1, _testDateTimeProvider.Now.AddDays(-1)).Should().Be(0);
    }
}
