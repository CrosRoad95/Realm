namespace RealmCore.Tests.Tests.Components;

public class PlayTimeComponentTests
{
    [Fact]
    public void TestIfCounterWorksCorrectly()
    {
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var dateTimeProvider = realmTestingServer.TestDateTimeProvider;

        var playTimeComponent = player1.AddComponent(new PlayTimeComponent(dateTimeProvider));
        var playTimeComponentWithInitialState = player2.AddComponent(new PlayTimeComponent(dateTimeProvider, 1000));
        playTimeComponent.PlayTime.Should().Be(TimeSpan.Zero);
        playTimeComponent.TotalPlayTime.Should().Be(TimeSpan.Zero);

        playTimeComponentWithInitialState.PlayTime.Should().Be(TimeSpan.Zero);
        playTimeComponentWithInitialState.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1000));

        dateTimeProvider.AddOffset(TimeSpan.FromSeconds(50));

        playTimeComponent.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        playTimeComponent.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(50));

        playTimeComponentWithInitialState.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        playTimeComponentWithInitialState.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(1050));
    }
}
