namespace RealmCore.Server.Logic.Components;

internal class DailyVisitsCounterComponentLogic : ComponentLogic<DailyVisitsCounterComponent>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DailyVisitsCounterComponentLogic(IECS ecs, IDateTimeProvider dateTimeProvider) : base(ecs)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void ComponentAdded(DailyVisitsCounterComponent dailyVisitsCounterComponent)
    {
        dailyVisitsCounterComponent.Update(_dateTimeProvider.Now);
    }
}
