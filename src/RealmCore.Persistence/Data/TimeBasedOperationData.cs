namespace RealmCore.Persistence.Data;

public sealed class TimeBasedOperationData
{
    public int Id { get; set; }
    public int Kind { get; set; }
    public int Status { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }

    public ICollection<TimeBasedOperationDataGroupUserData>? TimeBasedOperationDataGroupUser { get; set; }
}

public sealed class TimeBasedOperationGroupData
{
    public int Id { get; set; }
    public int Kind { get; set; }
    public int Limit { get; set; }
    public string? Metadata { get; set; }

    public ICollection<TimeBasedOperationDataGroupUserData>? TimeBasedOperationDataGroupUser { get; set; }
}

public sealed class TimeBasedOperationDataGroupUserData
{
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public int TimeBasedOperationId { get; set; }
    public string? Metadata { get; set; }

    public TimeBasedOperationGroupData? Group { get; set; }
    public TimeBasedOperationData? Operation { get; set; }
}
