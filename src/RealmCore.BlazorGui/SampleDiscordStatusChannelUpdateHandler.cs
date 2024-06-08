public class SampleDiscordStatusChannelUpdateHandler : IDiscordStatusChannelUpdateHandler
{
    public SampleDiscordStatusChannelUpdateHandler()
    {
    }

    public Task<string> HandleStatusUpdate(CancellationToken cancellationToken)
    {
        return Task.FromResult($"test 123 {DateTime.Now}");
    }
}
