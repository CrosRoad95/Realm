namespace RealmCore.Tests.Providers;

internal class TestDateTimeProvider : IDateTimeProvider
{
    private DateTime _now;
    public TestDateTimeProvider(DateTime? now = null)
    {
        _now = now ?? DateTime.Now;
    }

    public void AddOffset(TimeSpan timeSpan)
    {
        _now += timeSpan;
    }

    public DateTime Now => _now;
}
