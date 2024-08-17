namespace RealmCore.Server.Modules.Server;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    private readonly TimeZoneInfo _timeZoneInfo;

    public DateTimeProvider()
    {
        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
    }

    public DateTime Now => TimeZoneInfo.ConvertTime(DateTime.UtcNow, _timeZoneInfo);
}