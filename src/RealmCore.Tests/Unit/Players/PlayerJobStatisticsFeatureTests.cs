namespace RealmCore.Tests.Unit.Players;

public class PlayerJobStatisticsFeatureTests : RealmUnitTestingBase
{
    [Fact]
    public void AddPointsTimePlayedShouldWork()
    {
        var server = CreateServer();
        var player = CreatePlayer();
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
                JobId = 1,
                Points = 4,
                TimePlayed = 4
            },
            new UserJobStatisticsDto
            {
                JobId = 2,
                Points = 2,
                TimePlayed = 2
            }
        });
        #endregion
    }
}
