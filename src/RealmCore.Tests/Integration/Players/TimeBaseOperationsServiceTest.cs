namespace RealmCore.Tests.Integration.Players;

//public class TimeBaseOperationsServiceTest : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IAsyncDisposable
//{
//    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
//    private readonly RealmTestingPlayer _player;
//    private readonly IDateTimeProvider _dateTimeProvider;
//    private readonly TimeBaseOperationsService _timeBaseOperationsService;

//    public TimeBaseOperationsServiceTest(RealmTestingServerHostingFixtureWithPlayer fixture)
//    {
//        _fixture = fixture;
//        _player = _fixture.Player;
//        _dateTimeProvider = _fixture.Hosting.GetRequiredService<IDateTimeProvider>();
//        _timeBaseOperationsService = _fixture.Hosting.GetRequiredService<TimeBaseOperationsService>();
//    }

//    public ValueTask DisposeAsync()
//    {
//        throw new NotImplementedException();
//    }

//    public async Task test1()
//    {
//        int kind = 1;
//        int status = 1;
//        DateTime startDateTime = DateTime.UtcNow;
//        DateTime endDateTime = DateTime.Now;
//        string? input = "";
//        string? onput = "";

//        await _timeBaseOperationsService.Create(kind, status);
//    }
//}
