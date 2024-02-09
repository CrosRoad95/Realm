namespace RealmCore.Server.Modules.Server;

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTimeProvider()
    {

    }

    public DateTime Now => DateTime.Now;
}
