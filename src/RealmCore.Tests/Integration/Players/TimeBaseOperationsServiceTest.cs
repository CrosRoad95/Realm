namespace RealmCore.Tests.Integration.Players;

public class TimeBaseOperationsServiceTest : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>, IAsyncDisposable
{
    private readonly RealmTestingServerHostingFixtureWithUniquePlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly TimeBaseOperationsService _timeBaseOperationsService;

    public TimeBaseOperationsServiceTest(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _dateTimeProvider = _fixture.Hosting.GetRequiredService<TestDateTimeProvider>();
        _timeBaseOperationsService = _fixture.Hosting.GetRequiredService<TimeBaseOperationsService>();
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
    public async Task test1()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);
        var input = new SampleMetadata(1, "input");
        var output = new SampleMetadata(2, "output");

        var group = await _timeBaseOperationsService.CreateGroup(1, 3, new SampleMetadata(1337, "foobar"));

        var timeBasedOperation = await _timeBaseOperationsService.CreateForUser(group.Id, _player.UserId, kind, status, startDateTime, endDateTime, input, output);
        var operations = await _timeBaseOperationsService.GetByUserIdAndCategory(_player.UserId, 1);

        using var _ = new AssertionScope();
        operations.Should().HaveCount(1);
        var operation = operations[0];
        operation.Input.Should().BeOfType<SampleMetadata>();
        operation.Input.Should().BeEquivalentTo(input);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
