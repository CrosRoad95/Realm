using Org.BouncyCastle.Asn1.Cms;
using RealmCore.Persistence.Data;

namespace RealmCore.Server.Modules.World.WorldNodes;

internal sealed class WorldNodesHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly WorldNodesService _worldNodesService;
    private readonly ILogger<WorldNodesHostedService> _logger;

    public WorldNodesHostedService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, WorldNodesService worldNodesService, ILogger<WorldNodesHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _dateTimeProvider = dateTimeProvider;
        _worldNodesService = worldNodesService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();
        var worldNodeRepository = serviceScope.ServiceProvider.GetRequiredService<IWorldNodeRepository>();
        var worldNodesData = await worldNodeRepository.GetAll(cancellationToken);
        var worldNodeScheduledActionsData = await worldNodeRepository.GetAllScheduledActions(cancellationToken);

        Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var worldNodeData in worldNodesData)
        {
            var typeNameArray = worldNodeData.TypeName.Split(", ");
            var typeName = typeNameArray[0];
            var assemblyName = typeNameArray[1];
            var assembly = loadedAssemblies.Where(x => x.GetName().Name == assemblyName).FirstOrDefault()
                ?? throw new InvalidOperationException($"Assembly '{assemblyName}' not found.");
            var type = assembly.GetType(typeName)
                ?? throw new InvalidOperationException($"Type '{typeName}' not found.");

            try
            {
                var worldNode = await _worldNodesService.CreateFromData(type, worldNodeData);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to create worldNode id {worldNodeId}", worldNodeData.Id);
            }
        }

        foreach (var worldNodeScheduledActionData in worldNodeScheduledActionsData)
        {
            var actionData = worldNodeScheduledActionData.ActionData != null ? JsonConvert.DeserializeObject(worldNodeScheduledActionData.ActionData, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            }) : null;
            try
            {
                await _worldNodesService.ScheduleAction(worldNodeScheduledActionData.WorldNodeId, worldNodeScheduledActionData.ScheduledTime, actionData, worldNodeScheduledActionData.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule world node action id {worldNodeScheduledActionId}", worldNodeScheduledActionData.Id);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
