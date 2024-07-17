namespace RealmCore.Server.Modules.Commands;

public interface ICommandThrottlingPolicy
{
    bool Enabled { get; set; }
    void Execute(Action callback);
    Task ExecuteAsync(Func<CancellationToken, Task> callback, CancellationToken cancellationToken);
}

internal sealed class CommandThrottlingPolicy : ICommandThrottlingPolicy
{
    private readonly RateLimitPolicy _policy;
    private readonly AsyncRateLimitPolicy _policyAsync;

    public bool Enabled { get; set; } = true;

    public CommandThrottlingPolicy()
    {
        var maxCalls = 10;
        var timeSpan = TimeSpan.FromSeconds(10);

        _policy = Policy.RateLimit(maxCalls, timeSpan, maxCalls);
        _policyAsync = Policy.RateLimitAsync(maxCalls, timeSpan, maxCalls);
    }

    public void Execute(Action callback)
    {
        if (Enabled)
        {
            _policy.Execute(() =>
            {
                callback();
            });
        }
        else
        {
            callback();
        }
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> callback, CancellationToken cancellationToken)
    {
        if (Enabled)
        {
            await _policyAsync.ExecuteAsync(async (cancellationToken) =>
            {
                await callback(cancellationToken);
            }, cancellationToken);
        }
        else
        {
            await callback(cancellationToken);
        }
    }
}
