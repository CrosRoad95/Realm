namespace RealmCore.Server.Modules.Server;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeProvider()
    {

    }

    public DateTime Now => DateTime.Now;
}
