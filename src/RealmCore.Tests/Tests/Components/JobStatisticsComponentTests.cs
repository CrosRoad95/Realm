using RealmCore.Server.Structs;

namespace RealmCore.Tests.Tests.Components;

public class JobStatisticsComponentTests
{
    private readonly JobStatisticsComponent _jobStatisticsComponent;

    public JobStatisticsComponentTests()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        _jobStatisticsComponent = new(realmTestingServer.GetRequiredService<IDateTimeProvider>().Now);
        player.AddComponent(_jobStatisticsComponent);
    }

    [Fact]
    public void AddPointsTimePlayedShouldWork()
    {
        #region Arrange
        _jobStatisticsComponent.Reset();
        #endregion

        #region Act
        _jobStatisticsComponent.AddPoints(1, 2);
        _jobStatisticsComponent.AddPoints(1, 2);
        _jobStatisticsComponent.AddPoints(2, 2);
        _jobStatisticsComponent.AddTimePlayed(1, 2);
        _jobStatisticsComponent.AddTimePlayed(1, 2);
        _jobStatisticsComponent.AddTimePlayed(2, 2);
        #endregion

        #region Assert
        _jobStatisticsComponent.JobStatistics.Should().BeEquivalentTo(new Dictionary<int, JobStatistics>
        {
            [1] = new JobStatistics
            {
                jobId = 1,
                points = 4,
                sessionPoints = 4,
                sessionTimePlayed = 4,
                timePlayed = 4
            },
            [2] = new JobStatistics
            {
                jobId = 2,
                points = 2,
                sessionPoints = 2,
                sessionTimePlayed = 2,
                timePlayed = 2
            }
        });
        #endregion
    }
}
