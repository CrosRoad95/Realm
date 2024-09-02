using Microsoft.EntityFrameworkCore;

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

    public TimeBaseOperationsServiceTest(RealmTestingServerHostingFixtureWithUniquePlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _dateTimeProvider = _fixture.Hosting.GetRequiredService<TestDateTimeProvider>();
        _timeBaseOperationsService = _fixture.Hosting.GetRequiredService<TimeBaseOperationsService>();
        _sampleInputMetadata = new SampleMetadata(1, "input");
        _sampleOutputMetadata = new SampleMetadata(2, "output");
        _sampleMetadata = new SampleMetadata(3, "metadata");
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
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var groupUser = await _timeBaseOperationsService.CreateGroupForUser(_player.UserId, categoryId, 3, _sampleMetadata);

        var foundGroup = await _timeBaseOperationsService.GetGroupById(groupUser.GroupId);
        var foundGroups1 = await _timeBaseOperationsService.GetGroupsByUserId(_player.UserId);
        var foundGroups2 = await _timeBaseOperationsService.GetGroupsByCategoryId(categoryId);

        var timeBasedOperation = await _timeBaseOperationsService.CreateForUser(groupUser.GroupId, _player.UserId, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        var operations = await _timeBaseOperationsService.GetOperationsByUserIdAndCategory(_player.UserId, 1);
        var count = await _timeBaseOperationsService.CountOperationsByGroupId(groupUser.GroupId);

        using var _ = new AssertionScope();
        foundGroup.Should().NotBeNull();
        foundGroups1.Should().HaveCount(1);
        foundGroups2.Where(x => x.Id == groupUser.GroupId).Should().HaveCount(1);

        count.Should().Be(1);
        operations.Should().HaveCount(1);
        var operation = operations[0];
        operation.Input.Should().BeOfType<SampleMetadata>();
        operation.Input.Should().BeEquivalentTo(_sampleInputMetadata);
    }
    
    [Fact]
    public async Task GroupLimitShouldWork()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var limit = 2;
        var groupUser = await _timeBaseOperationsService.CreateGroupForUser(_player.UserId, 1, limit, new SampleMetadata(1337, "foobar"));

        var timeBasedOperation1 = await _timeBaseOperationsService.CreateForUser(groupUser.GroupId, _player.UserId, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        var timeBasedOperation2 = await _timeBaseOperationsService.CreateForUser(groupUser.GroupId, _player.UserId, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        var timeBasedOperation3 = await _timeBaseOperationsService.CreateForUser(groupUser.GroupId, _player.UserId, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);

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

        var timeBasedOperation = await _timeBaseOperationsService.CreateForUser(-1, _player.UserId, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);

        timeBasedOperation.Should().BeNull();
    }
    
    [Fact]
    public async Task OperationShouldBeDeletable()
    {
        int kind = 1;
        int status = 1;
        DateTime startDateTime = _dateTimeProvider.Now;
        DateTime endDateTime = _dateTimeProvider.Now.AddDays(3);

        var groupUser = await _timeBaseOperationsService.CreateGroupForUser(_player.UserId, 1, 2, _sampleMetadata);

        var operation = await _timeBaseOperationsService.CreateForUser(groupUser.GroupId, _player.UserId, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        await _timeBaseOperationsService.DeleteOperation(operation!.Id);
        var operations = await _timeBaseOperationsService.GetOperationsByUserIdAndCategory(_player.UserId, 1);
        var count = await _timeBaseOperationsService.CountOperationsByGroupId(groupUser.GroupId);

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

        var groupUser = await _timeBaseOperationsService.CreateGroupForUser(_player.UserId, 1, 2, _sampleMetadata);

        var operation = await _timeBaseOperationsService.CreateForUser(groupUser.GroupId, _player.UserId, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);

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

        var groupUser = await _timeBaseOperationsService.CreateGroupForUser(_player.UserId, 1, 2, _sampleMetadata);

        var operation = await _timeBaseOperationsService.CreateForUser(groupUser.GroupId, _player.UserId, kind, status, startDateTime, endDateTime, _sampleInputMetadata, _sampleOutputMetadata);
        var updated = await _timeBaseOperationsService.SetStatus(operation!.Id, 2);
        var operations = await _timeBaseOperationsService.GetOperationsByUserIdAndCategory(_player.UserId, 1);

        using var _ = new AssertionScope();
        updated.Should().BeTrue();
        operations.Where(x => x.Id == operation!.Id).First().Status.Should().Be(2);
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.Hosting.GetRequiredService<IDb>().TimeBaseOperations.ExecuteDeleteAsync();
    }
}
