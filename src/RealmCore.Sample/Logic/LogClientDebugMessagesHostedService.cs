namespace RealmCore.Sample.Logic;

internal sealed class LogClientDebugMessagesHostedService : IHostedService
{
    private readonly ILogger<LogClientDebugMessagesHostedService> _logger;
    private readonly IClientInterfaceService _clientInterfaceService;
    private readonly IElementFactory _elementFactory;

    public LogClientDebugMessagesHostedService(IClientInterfaceService clientInterfaceService, ILogger<LogClientDebugMessagesHostedService> logger, IElementFactory elementFactory)
    {
        _clientInterfaceService = clientInterfaceService;
        _elementFactory = elementFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _clientInterfaceService.OnClientDebugMessages += HandleClientErrorMessage;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _clientInterfaceService.OnClientDebugMessages -= HandleClientErrorMessage;
        return Task.CompletedTask;
    }

    private void HandleClientErrorMessage(Player player, ClientDebugMessage[] debugMessages)
    {
        using var _ = _logger.BeginElement(player);
        var playerName = player.Name;
        foreach (var debugMessage in debugMessages)
        {
            switch (debugMessage.Level)
            {
                case 0: // Custom
                case 3: // Information
                    if (debugMessage.Line > 0)
                        _logger.LogTrace("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, debugMessage.Level, debugMessage.Message, debugMessage.File, debugMessage.Line);
                    else
                        _logger.LogTrace("Clientside: {player} ({level}): {message}", playerName, debugMessage.Level, debugMessage.Message);
                    break;
                case 2: // Warning
                    if (debugMessage.Line > 0)
                        _logger.LogWarning("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, debugMessage.Level, debugMessage.Message, debugMessage.File, debugMessage.Line);
                    else
                        _logger.LogWarning("Clientside: {player} ({level}): {message}", playerName, debugMessage.Level, debugMessage.Message);
                    break;
                default: // Error or something else
                    if (debugMessage.Line > 0)
                        _logger.LogError("Clientside: {player} ({level}): {message} in {file}:{line}", playerName, debugMessage.Level, debugMessage.Message, debugMessage.File, debugMessage.Line);
                    else
                        _logger.LogError("Clientside: {player} ({level}): {message}", playerName, debugMessage.Level, debugMessage.Message);
                    break;
            }
        }

    }
}
