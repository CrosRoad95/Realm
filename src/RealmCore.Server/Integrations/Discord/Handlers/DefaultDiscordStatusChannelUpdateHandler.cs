namespace RealmCore.Server.Integrations.Discord.Handlers;

public class DefaultDiscordStatusChannelUpdateHandler : IDiscordStatusChannelUpdateHandler
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DefaultDiscordStatusChannelUpdateHandler(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<string> HandleStatusUpdate(CancellationToken cancellationToken)
    {
        return Task.FromResult($"test 123 {_dateTimeProvider.Now}");
    }
}
