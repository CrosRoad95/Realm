namespace RealmCore.Tests.Unit.Players;

public class PlayerJobStatisticsFeatureTests
{
    [Fact]
    public async Task AddPointsTimePlayedShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var obj = hosting.CreateFocusableObject();
        var statistics = player.JobStatistics;

        #region Act
        statistics.AddPoints(1, 2);
        statistics.AddPoints(1, 2);
        statistics.AddPoints(2, 2);
        statistics.AddTimePlayed(1, 2);
        statistics.AddTimePlayed(1, 2);
        statistics.AddTimePlayed(2, 2);
        #endregion

        #region Assert
        statistics.Should().BeEquivalentTo(new List<UserJobStatisticsDto>
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
        #endregion
    }
}
