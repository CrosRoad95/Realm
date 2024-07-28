using RealmCore.Server.Modules.Players.Discord;
using RealmCore.Server.Modules.Vehicles.Access;

namespace RealmCore.BlazorGui.Logic;

internal sealed class TestHostedService : PlayerLifecycle, IHostedService
{
    private readonly IElementFactory _elementFactory;
    private readonly ISchedulerService _schedulerService;
    private readonly ILogger<TestHostedService> _logger;
    private readonly ChatBox _chatBox;
    private readonly INametagsService _nametagsService;
    private readonly IVehiclesAccessService _vehiclesAccessService;

    public TestHostedService(PlayersEventManager playersEventManager, IElementFactory elementFactory, ISchedulerService schedulerService, ILogger<TestHostedService> logger, ChatBox chatBox, INametagsService nametagsService, IVehiclesAccessService vehiclesAccessService) : base(playersEventManager)
    {
        _elementFactory = elementFactory;
        _schedulerService = schedulerService;
        _logger = logger;
        _chatBox = chatBox;
        _nametagsService = nametagsService;
        _vehiclesAccessService = vehiclesAccessService;

        //_vehiclesAccessService.CanNotEnter += HandleCanNotEnter;
        //SchedulerTests();
    }

    private bool HandleCanNotEnter(Ped ped, RealmVehicle vehicle, byte seat)
    {
        if (seat == 0)
            return true;
        return false;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var marker = _elementFactory.CreateMarker(new Location(335.50684f, -83.71094f, 1.4105641f), MarkerType.Cylinder, 1, Color.Red);
        marker.Size = 4;

        var ped = _elementFactory.CreatePed(new Location(232.93f, -73.18f, 1.43f), PedModel.Cj);
        ped.Nametag.Text = "sample nametag";
        var _ = Task.Run(async () =>
        {
            int i = 0;
            while (true)
            {
                ped.Nametag.Text = $"sample nametag {i++}";
                await Task.Delay(1000);
            }
        }, CancellationToken.None);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.FocusedElementChanged += HandleFocusedElementChanged;
        player.FocusedVehiclePartChanged += HandleFocusedVehiclePartChanged;
        player.DiscordRichPresence.Ready += HandleDiscordRichPresenceReady;
    }

    private void HandleDiscordRichPresenceReady(IDiscordRichPresenceFeature discordRichPresence)
    {
        _logger.LogInformation("Discord ready: {discordUserId}", discordRichPresence.UserId);
    }

    void HandleFocusedElementChanged(RealmPlayer arg1, Element? previous, Element? current)
    {
        //_chatBox.Output($"Changed focused element to: {current?.ToString() ?? "<brak>"}");
    }

    private void HandleFocusedVehiclePartChanged(RealmPlayer player, string? arg2, string? arg3)
    {
        _chatBox.OutputTo(player, $"Changed focused vehicle part from {arg2 ?? "<brak>"} to: {arg3 ?? "<brak>"}");
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
