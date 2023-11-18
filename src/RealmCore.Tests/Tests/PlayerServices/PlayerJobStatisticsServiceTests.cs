namespace RealmCore.Tests.Tests.PlayerServices;

public class PlayerJobStatisticsServiceTests
{
    [Fact]
    public void AddPointsTimePlayedShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
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
        statistics.Should().BeEquivalentTo(new List<UserJobStatisticsDTO>
        {
            new UserJobStatisticsDTO
            {
                JobId = 1,
                Points = 4,
                TimePlayed = 4
            },
            new UserJobStatisticsDTO
            {
                JobId = 2,
                Points = 2,
                TimePlayed = 2
            }
        });
        #endregion
    }
}
