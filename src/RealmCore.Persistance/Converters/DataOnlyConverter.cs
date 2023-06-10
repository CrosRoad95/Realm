using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RealmCore.Persistence.Converters;

// https://stackoverflow.com/questions/69146423/date-only-cannot-be-mapped-sql-server-2019
internal class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    /// <summary>
    /// Creates a new instance of this converter.
    /// </summary>
    public DateOnlyConverter() : base(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d))
    { }
}