using RealmCore.Server.Modules.World.WorldNodes;

namespace RealmCore.BlazorGui.Modules.World;

public record struct SampleState(int SampleValue);
public record struct SampleAction(int SampleValue);

public class SampleNodeInteraction : Interaction
{
    public SampleNode SampleNode { get; }

    public SampleNodeInteraction(SampleNode sampleNode)
    {
        SampleNode = sampleNode;
    }
}

public class SampleNode : WorldNode
{
    private readonly IElementFactory _elementFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    private RealmWorldObject? _worldObject;

    public SampleNode(IElementFactory elementFactory, IDateTimeProvider dateTimeProvider)
    {
        _elementFactory = elementFactory;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override Task Initialized()
    {
        _worldObject = _elementFactory.CreateFocusableObject(new Location(Position), (ObjectModel)1337);
        _worldObject.Interaction = new SampleNodeInteraction(this);
        return Task.CompletedTask;
    }

    public async Task HandleSampleInteraction()
    {
        await UpdateMetadata<SampleState>(x =>
        {
            x.SampleValue++;
            return x;
        });

        await ScheduleAction(_dateTimeProvider.Now.AddSeconds(5), new SampleAction
        {
            SampleValue = 1234
        });
        await ScheduleAction(_dateTimeProvider.Now.AddSeconds(30), new SampleAction
        {
            SampleValue = 1337
        });
        Console.WriteLine("Schedule at {0}", _dateTimeProvider.Now);
    }

    protected override Task ProcessAction(object? data)
    {
        switch (data)
        {
            case SampleAction sampleAction:
                Console.WriteLine("Executed schedule at {0}, param: {1}", _dateTimeProvider.Now, sampleAction.SampleValue);
                break;
        }

        return Task.CompletedTask;
    }

    protected override void Dispose()
    {
        _worldObject?.Destroy();
    }
}

public class WorldNodes : IHostedService
{
    private readonly WorldNodesService _worldNodesService;

    public WorldNodes(WorldNodesService worldNodesService)
    {
        _worldNodesService = worldNodesService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {

        return Task.CompletedTask;
    }
}
