namespace RealmCore.Server.Modules.Commands;

public interface ICommandThrottlingPolicy
{
    void Execute(Action callback);
    Task ExecuteAsync(Func<CancellationToken, Task> callback, CancellationToken cancellationToken);
}

internal sealed class CommandThrottlingPolicy : ICommandThrottlingPolicy
{
    Polly.RateLimit.RateLimitPolicy _policy;
    Polly.RateLimit.AsyncRateLimitPolicy _policyAsync;

    public CommandThrottlingPolicy()
    {
        var maxCalls = 10;
        var timeSpan = TimeSpan.FromSeconds(10);

        _policy = Policy.RateLimit(maxCalls, timeSpan, maxCalls);
        _policyAsync = Policy.RateLimitAsync(maxCalls, timeSpan, maxCalls);
    }

    public void Execute(Action callback)
    {
        _policy.Execute(() =>
        {
            callback();
        });
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> callback, CancellationToken cancellationToken)
    {
        await _policyAsync.ExecuteAsync(async (cancellationToken) =>
        {
            await callback(cancellationToken);
        }, cancellationToken);
    }
}
