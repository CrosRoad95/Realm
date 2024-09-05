namespace RealmCore.Tests.Integration.Players;

public class TimeBaseOperationsServiceTest : IClassFixture<RealmTestingServerHostingFixtureWithUniquePlayer>, IAsyncDisposable
{
    private readonly RealmTestingServerHostingFixtureWithUniquePlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly TimeBaseOperationsService _timeBaseOperationsService;

    private readonly SampleMetadata _sampleInputMetadata;
    private readonly SampleMetadata _sampleOutputMetadata;
    private readonly SampleMetadata _sampleMetadata;
    private readonly SampleMetadata _sampleMetadata2;

    public TimeBaseOperationsServiceTest(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _dateTimeProvider = _fixture.Hosting.GetRequiredService<TestDateTimeProvider>();
        _timeBaseOperationsService = _fixture.Hosting.GetRequiredService<TimeBaseOperationsService>();
        _sampleInputMetadata = new SampleMetadata(1, "input");
        _sampleOutputMetadata = new SampleMetadata(2, "output");
        _sampleMetadata = new SampleMetadata(3, "metadata");
        _sampleMetadata2 = new SampleMetadata(4, "new metadata");
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
    public async Task TimeBaseOperationsShouldWork()
    {
        int kind = 1;
        int status = 1;
        int categoryId = 1;
        int limit = 3;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var group = await _timeBaseOperationsService.CreateGroup(categoryId, limit, _sampleMetadata);

        var foundLimit = await _timeBaseOperationsService.GetGroupLimitById(group.Id);
        var foundGroup = await _timeBaseOperationsService.GetGroupById(group.Id);

        var timeBasedOperation = await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        var operations = await _timeBaseOperationsService.GetOperationsGroupId(group.Id);
        var count = await _timeBaseOperationsService.CountOperationsByGroupId(group.Id);

        using var _ = new AssertionScope();
        foundLimit.Should().Be(limit);
        foundGroup.Should().NotBeNull();

        count.Should().Be(1);
        operations.Should().HaveCount(1);
        var operation = operations[0];
        operation.GetInput<SampleMetadata>().Should().BeOfType<SampleMetadata>();
        operation.GetInput<SampleMetadata>().Should().BeEquivalentTo(_sampleInputMetadata);
    }
    
    [Fact]
    public async Task OperationEndDateShouldBeAfterStartDate()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(-3);

        var group = await _timeBaseOperationsService.CreateGroup(1, 1, new SampleMetadata(1337, "foobar"));

        var act = async () => await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata, _sampleMetadata);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [Fact]
    public async Task OperationMetadataShouldWork()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var limit = 2;
        var group = await _timeBaseOperationsService.CreateGroup(1, limit, new SampleMetadata(1337, "foobar"));

        var timeBasedOperation = await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata, _sampleMetadata);
        var foundMetadata1 = await _timeBaseOperationsService.GetOperationMetadata<SampleMetadata>(timeBasedOperation!.Id);
        var setMetadata = await _timeBaseOperationsService.SetOperationMetadata(timeBasedOperation.Id, _sampleMetadata2);
        var foundMetadata2 = await _timeBaseOperationsService.GetOperationMetadata<SampleMetadata>(timeBasedOperation!.Id);

        using var _ = new AssertionScope();
        timeBasedOperation!.GetMetadata<SampleMetadata>().Should().BeEquivalentTo(_sampleMetadata);
        foundMetadata1.Should().BeEquivalentTo(_sampleMetadata);
        setMetadata.Should().BeTrue();
        foundMetadata2.Should().BeEquivalentTo(_sampleMetadata2);
    }
    
    [Fact]
    public async Task GroupLimitShouldWork()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var limit = 2;
        var group = await _timeBaseOperationsService.CreateGroup(1, limit, new SampleMetadata(1337, "foobar"));

        var timeBasedOperation1 = await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        var timeBasedOperation2 = await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        var timeBasedOperation3 = await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);

        using var _ = new AssertionScope();
        timeBasedOperation1.Should().NotBeNull();
        timeBasedOperation2.Should().NotBeNull();
        timeBasedOperation3.Should().BeNull();
    }
    
    [Fact]
    public async Task YouCanNotCreateOperationForNonExistingGroup()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var timeBasedOperation = await _timeBaseOperationsService.CreateOperation(-1, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);

        timeBasedOperation.Should().BeNull();
    }
    
    [Fact]
    public async Task OperationShouldBeDeletable()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var group = await _timeBaseOperationsService.CreateGroup(1, 2, _sampleMetadata);

        var operation = await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        await _timeBaseOperationsService.DeleteOperation(operation!.Id);
        var operations = await _timeBaseOperationsService.GetOperationsGroupId(group.Id);
        var count = await _timeBaseOperationsService.CountOperationsByGroupId(group.Id);

        using var _ = new AssertionScope();
        operations.Where(x => x.Id == operation!.Id).Should().BeEmpty();
        count.Should().Be(0);
    }
    
    [Fact]
    public async Task OperationShouldBeCompletable()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var group = await _timeBaseOperationsService.CreateGroup(1, 2, _sampleMetadata);

        var operation = await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);

        using var _ = new AssertionScope();
        operation!.IsCompleted(_dateTimeProvider).Should().BeFalse();
        _dateTimeProvider.Add(TimeSpan.FromDays(3));
        operation!.IsCompleted(_dateTimeProvider).Should().BeTrue();
    }
    
    [Fact]
    public async Task YouShouldBeAbleToChangeStatus()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var group = await _timeBaseOperationsService.CreateGroup(1, 2, _sampleMetadata);

        var operation = await _timeBaseOperationsService.CreateOperation(group.Id, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        var updated = await _timeBaseOperationsService.SetStatus(operation!.Id, 2);
        var operations = await _timeBaseOperationsService.GetOperationsGroupId(group.Id);

        using var _ = new AssertionScope();
        updated.Should().BeTrue();
        operations.First().Status.Should().Be(2);
    }
    
    [Fact]
    public async Task YouCanChangeGroupMetadata()
    {
        var group = await _timeBaseOperationsService.CreateGroup(1, 2, _sampleMetadata);
        var metadata1 = await _timeBaseOperationsService.GetGroupMetadata<SampleMetadata>(group.Id);
        var changedMetadata = await _timeBaseOperationsService.SetGroupMetadata(group.Id, _sampleMetadata2);
        var metadata2 = await _timeBaseOperationsService.GetGroupMetadata<SampleMetadata>(group.Id);

        using var _ = new AssertionScope();
        group.GetMetadata<SampleMetadata>().Should().BeEquivalentTo(_sampleMetadata);
        metadata1.Should().BeEquivalentTo(_sampleMetadata);
        changedMetadata.Should().BeTrue();
        metadata2.Should().BeEquivalentTo(_sampleMetadata2);
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.Hosting.GetRequiredService<IDb>().TimeBaseOperations.ExecuteDeleteAsync();
    }
}
