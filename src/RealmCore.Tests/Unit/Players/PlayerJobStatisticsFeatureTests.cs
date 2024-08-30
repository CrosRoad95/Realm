namespace RealmCore.Tests.Unit.Players;

public class PlayerJobStatisticsFeatureTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerJobStatisticsFeature _jobStatistics;

    public PlayerJobStatisticsFeatureTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
        _jobStatistics = _player.JobStatistics;
    }

    [Fact]
    public void AddPointsTimePlayedShouldWork()
    {
        var obj = _hosting.CreateFocusableObject();

        _jobStatistics.AddPoints(1, 2);
        _jobStatistics.AddPoints(1, 2);
        _jobStatistics.AddPoints(2, 2);
        _jobStatistics.AddTimePlayed(1, 2);
        _jobStatistics.AddTimePlayed(1, 2);
        _jobStatistics.AddTimePlayed(2, 2);

        _jobStatistics.Should().BeEquivalentTo(new List<UserJobStatisticsDto>
        {
            new UserJobStatisticsDto
            {
                UserId = -1,
                JobId = 1,
                Points = 4,
                TimePlayed = 4
            },
            new UserJobStatisticsDto
            {
                UserId = -1,
                JobId = 2,
                Points = 2,
                TimePlayed = 2
            }
        }, options =>
        {
            options.Excluding(ctx => ctx.UserId);

            return options;
        });
    }
}
