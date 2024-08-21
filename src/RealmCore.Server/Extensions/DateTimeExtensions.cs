namespace RealmCore.Server.Extensions;

public static class DateTimeExtensions
{
    public static bool AreSameDay(this DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1.Year == dateTime2.Year && dateTime1.Month == dateTime2.Month && dateTime1.Day == dateTime2.Day;
    }
}
