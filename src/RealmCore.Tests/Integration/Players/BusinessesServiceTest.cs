using RealmCore.Server.Modules.Businesses;

namespace RealmCore.Tests.Integration.Players;

public class BusinessesServiceTest : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>, IAsyncDisposable
{
    private readonly RealmTestingServerHostingFixtureWithUniquePlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly BusinessesService _businessesService;
    private readonly TimeBaseOperationsService _timeBaseOperationsService;

    private readonly SampleMetadata _sampleMetadata;
    private readonly SampleMetadata _sampleMetadata2;

    public BusinessesServiceTest(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _dateTimeProvider = _fixture.Hosting.GetRequiredService<TestDateTimeProvider>();
        _businessesService = _fixture.Hosting.GetRequiredService<BusinessesService>();
        _timeBaseOperationsService = _fixture.Hosting.GetRequiredService<TimeBaseOperationsService>();
        _sampleMetadata = new SampleMetadata(1, "metadata");
        _sampleMetadata2 = new SampleMetadata(2, "new metadata");
    }

    public class SampleMetadata
    {
        public int Number { get; set; }
        public string String { get; set; }

        public SampleMetadata(int number, string @string)
        {
            Number = number;
            String = @string;
        }
    }

    [Fact]
    public async Task YouShouldBeAbleToCreateBusinessAndAddUserToIt()
    {
        var business = await _businessesService.Create(1, _sampleMetadata);
        var added1 = await _businessesService.AddUser(business.Id, _player.UserId);
        var added2 = await _businessesService.AddUser(business.Id, _player.UserId);
        var users = await _businessesService.GetUsersById(business.Id);

        using var _ = new AssertionScope();

        business.Should().NotBeNull();
        added1.Should().BeTrue();
        added2.Should().BeFalse();
        users.Should().BeEquivalentTo([_player.UserId]);
    }

    [Fact]
    public async Task YouShouldBeAbleToCreateBusinessAndAddUserToIt2()
    {
        var business = await _businessesService.Create(1, _sampleMetadata);
        var group = await _timeBaseOperationsService.CreateGroup(1, 1);
        var added1 = await _businessesService.AddBusinessToTimeBaseGroup(business.Id, group.Id);
        var added2 = await _businessesService.AddBusinessToTimeBaseGroup(business.Id, group.Id);
        var groups = await _timeBaseOperationsService.GetGroupsByBusinessId(business.Id);
        var added3 = await _businessesService.AddUser(business.Id, _player.UserId);
        var businesses1 = await _businessesService.GetByUserId(_player.UserId);
        var businesses2 = await _businessesService.GetByUserIdAndCategory(_player.UserId, 1);

        using var _ = new AssertionScope();
        business.Should().NotBeNull();
        added1.Should().BeTrue();
        added2.Should().BeFalse();
        added3.Should().BeTrue();
        groups.SelectMany(x => x.Businesses.Select(y => y.Id)).Should().BeEquivalentTo([business.Id]);
        businesses1.Select(x => x.Id).Should().Contain(business.Id);
        businesses2.Select(x => x.Id).Should().Contain(business.Id);
    }

    [Fact]
    public async Task StatisticsShouldWork()
    {
        var business = await _businessesService.Create(1, _sampleMetadata);
        var result1 = await _businessesService.IncreaseStatistic(business.Id, 1, 1);
        var result2 = await _businessesService.IncreaseStatistic(business.Id, 1, 1);
        var result3 = await _businessesService.IncreaseStatistic(business.Id, 2, 1);
        var results = await _businessesService.GetStatistics(business.Id);

        using var _ = new AssertionScope();
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
        results.Should().BeEquivalentTo(new Dictionary<int, float>
        {
            [1] = 2,
            [2] = 1
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.Hosting.GetRequiredService<IDb>().Businesses.ExecuteDeleteAsync();
    }
}
