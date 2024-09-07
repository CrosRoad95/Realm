namespace RealmCore.Persistence.Data;

public class BusinessData
{
    public int Id { get; set; }
    public int Category { get; set; }
    public string? Metadata { get; set; }

    public ICollection<TimeBaseOperationGroupBusinessData> TimeBasedOperations { get; set; } = [];
    public ICollection<BusinessStatisticData> Statistics { get; set; } = [];
    public ICollection<BusinessUserData> Users { get; set; } = [];
    public ICollection<BusinessEventData> Events { get; set; } = [];
}

public class BusinessUserData
{
    public int BusinessId { get; set; }
    public int UserId { get; set; }
    public string? Metadata { get; set; }

    public BusinessData? Business { get; set; }
    public UserData? User { get; set; }
}

public class BusinessStatisticData
{
    public int BusinessId { get; set; }
    public int StatisticId { get; set; }
    public float Value { get; set; }

    public BusinessData? Business { get; set; }
}

public sealed class BusinessEventData : EventDataBase
{
    public int BusinessId { get; set; }
}
