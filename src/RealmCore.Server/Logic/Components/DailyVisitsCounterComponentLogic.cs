namespace RealmCore.Server.Logic.Components;

internal sealed class DailyVisitsCounterComponentLogic : ComponentLogic<DailyVisitsCounterComponent>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DailyVisitsCounterComponentLogic(IEntityEngine ecs, IDateTimeProvider dateTimeProvider) : base(ecs)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void ComponentAdded(DailyVisitsCounterComponent dailyVisitsCounterComponent)
    {
        dailyVisitsCounterComponent.Update(_dateTimeProvider.Now);
    }
}
