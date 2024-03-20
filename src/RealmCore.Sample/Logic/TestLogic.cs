namespace RealmCore.Sample.Logic;

internal class TestLogic : PlayerLifecycle
{
    private readonly IElementFactory _elementFactory;
    private readonly ISchedulerService _schedulerService;
    private readonly ILogger<TestLogic> _logger;
    private readonly ChatBox _chatBox;

    public TestLogic(IElementFactory elementFactory, ISchedulerService schedulerService, ILogger<TestLogic> logger, ChatBox chatBox, MtaServer mtaServer) : base(mtaServer)
    {
        var marker = elementFactory.CreateMarker(new Location(335.50684f, -83.71094f, 1.4105641f), MarkerType.Cylinder, 1, Color.Red);
        marker.Size = 4;
        marker.CollisionDetection.AddRule<MustBeVehicleRule>();
        _elementFactory = elementFactory;
        _schedulerService = schedulerService;
        _logger = logger;
        _chatBox = chatBox;
        SchedulerTests();
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.FocusedVehiclePartChanged += HandleFocusedVehiclePartChanged;
    }

    private void HandleFocusedVehiclePartChanged(RealmPlayer player, string? arg2, string? arg3)
    {
        _chatBox.OutputTo(player, $"Changed focused vehicle element to: {arg3 ?? "<brak>"}");
    }

    private void SchedulerTests()
    {
        var cts = new CancellationTokenSource();
        var scheduledJob = _schedulerService.ScheduleJob(() =>
        {
            Console.WriteLine("Hello {0}", DateTime.Now);
            return Task.CompletedTask;
        }, TimeSpan.FromSeconds(3), cts.Token);

        var t = DateTime.Now.AddSeconds(15);
        var t2 = DateTime.Now.AddSeconds(16);
        var t3 = DateTime.Now.AddSeconds(20);
        var scheduledAt = _schedulerService.ScheduleJobAt(() =>
        {
            Console.WriteLine("Time1 {0} = {1}", DateTime.Now, t3);
            return Task.CompletedTask;
        }, t, CancellationToken.None);
        scheduledAt.Reschedule(t3);

        _schedulerService.ScheduleJobAt(() =>
        {
            Console.WriteLine("Time2 {0} = {1}", DateTime.Now, t);
            return Task.CompletedTask;
        }, t2, cts.Token);

        _schedulerService.ScheduleJobAt(() =>
        {
            Console.WriteLine("Cron every 2s {0}", DateTime.Now);
            return Task.CompletedTask;
        }, "*/2 * * * * ? *", cts.Token);

        _schedulerService.ScheduleJobOnce(() =>
        {
            Console.WriteLine("ONCE");
            return Task.CompletedTask;
        }, TimeSpan.FromSeconds(15));
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(1000 * 8);
                scheduledJob.Reschedule(TimeSpan.FromSeconds(1));
                await Task.Delay(1000 * 3);
                await cts.CancelAsync();
                await Task.Delay(1000 * 3);
                scheduledJob.Reschedule(TimeSpan.FromSeconds(1));
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        });
    }
}
