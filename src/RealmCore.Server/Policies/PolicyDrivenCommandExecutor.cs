namespace RealmCore.Server.Policies;

internal class PolicyDrivenCommandExecutor : IPolicyDrivenCommandExecutor
{
    Polly.RateLimit.RateLimitPolicy<Context> _policy;
    Polly.RateLimit.AsyncRateLimitPolicy<Context> _policyAsync;

    public PolicyDrivenCommandExecutor()
    {
        var maxCallsPerUser = 10;
        var timeSpan = TimeSpan.FromSeconds(10);

        _policy = Policy.RateLimit<Context>(maxCallsPerUser, timeSpan, maxCallsPerUser);
        _policyAsync = Policy.RateLimitAsync<Context>(maxCallsPerUser, timeSpan, maxCallsPerUser);
    }

    public void Execute(Action callback, string operationKey)
    {
        _policy.Execute((ctx) =>
        {
            callback();
            return ctx;
        }, new Context(operationKey));
    }

    public async Task ExecuteAsync(Func<Task> callback, string operationKey)
    {
        await _policyAsync.ExecuteAsync(async (ctx) =>
        {
            await callback();
            return ctx;
        }, new Context(operationKey));
    }
}
