using Realm.Common.Providers;

namespace Realm.Server.Providers;

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTimeProvider()
    {

    }

    public DateTime Now => DateTime.Now;
}
