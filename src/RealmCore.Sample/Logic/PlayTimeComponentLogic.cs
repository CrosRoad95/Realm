namespace RealmCore.Sample.Logic;

internal class PlayTimeComponentLogic : ComponentLogic<PlayTimeComponent>
{
    private readonly ILogger<PlayTimeComponentLogic> _logger;

    public PlayTimeComponentLogic(IElementFactory elementFactory, ILogger<PlayTimeComponentLogic> logger) : base(elementFactory)
    {
        _logger = logger;
    }

    protected override void ComponentAdded(PlayTimeComponent playTimeComponent)
    {
        playTimeComponent.MinutePlayed += HandleMinutePlayed;
        playTimeComponent.MinuteTotalPlayed += HandleMinuteTotalPlayed;
    }

    protected override void ComponentDetached(PlayTimeComponent playTimeComponent)
    {
        playTimeComponent.MinutePlayed -= HandleMinutePlayed;
        playTimeComponent.MinuteTotalPlayed -= HandleMinuteTotalPlayed;
    }

    private void HandleMinutePlayed(PlayTimeComponent playTimeComponent)
    {
        _logger.LogInformation("Minute played, time play: {a}, total: {b}", playTimeComponent.PlayTime, playTimeComponent.TotalPlayTime);
    }

    private void HandleMinuteTotalPlayed(PlayTimeComponent playTimeComponent)
    {
        _logger.LogInformation("Minute total played, time play: {a}, total: {b}", playTimeComponent.PlayTime, playTimeComponent.TotalPlayTime);
    }
}
