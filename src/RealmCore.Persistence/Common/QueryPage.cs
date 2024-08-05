namespace RealmCore.Persistence.Common;

public record struct QueryPage(int page, int limit);

public record struct DateRange(DateTime from, DateTime to);