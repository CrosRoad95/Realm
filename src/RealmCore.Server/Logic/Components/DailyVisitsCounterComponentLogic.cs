namespace RealmCore.Server.Logic.Components;

internal sealed class DailyVisitsCounterComponentLogic : ComponentLogic<DailyVisitsCounterComponent>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public DailyVisitsCounterComponentLogic(IElementFactory elementFactory, IDateTimeProvider dateTimeProvider) : base(elementFactory)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void ComponentAdded(DailyVisitsCounterComponent dailyVisitsCounterComponent)
    {
        dailyVisitsCounterComponent.Update(_dateTimeProvider.Now);
    }
}
