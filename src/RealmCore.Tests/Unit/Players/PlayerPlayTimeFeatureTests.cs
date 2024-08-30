namespace RealmCore.Tests.Unit.Players;

public class PlayerPlayTimeFeatureTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly PlayerPlayTimeFeature _playTime;

    public PlayerPlayTimeFeatureTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
        _dateTimeProvider = _hosting.DateTimeProvider;
        _playTime = _player.PlayTime;
    }

    [Fact]
    public void TestIfCounterWorksCorrectly()
    {
        _playTime.PlayTime.Should().Be(TimeSpan.Zero);
        _playTime.TotalPlayTime.Should().Be(TimeSpan.Zero);

        _dateTimeProvider.Add(TimeSpan.FromSeconds(50));

        _playTime.PlayTime.Should().Be(TimeSpan.FromSeconds(50));
        _playTime.TotalPlayTime.Should().Be(TimeSpan.FromSeconds(50));
    }

    [Fact]
    public void CategoryPlayTimeShouldWork1()
    {
        _playTime.Category = 1;
        _hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));
        _playTime.Category = 2;
        _hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));

        _playTime.ToArray().Should().BeEquivalentTo([
            new PlayerPlayTimeDto(1, TimeSpan.FromSeconds(30)),
            new PlayerPlayTimeDto(2, TimeSpan.FromSeconds(30))
        ]);
    }

    [Fact]
    public void CategoryPlayTimeShouldWork2()
    {
        _playTime.Category = 1;
        _hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));
        _hosting.DateTimeProvider.Add(TimeSpan.FromSeconds(30));

        _playTime.GetByCategory(1).Should().Be(TimeSpan.FromMinutes(1));
    }

    public void Dispose()
    {
        _playTime.Clear();
    }
}
