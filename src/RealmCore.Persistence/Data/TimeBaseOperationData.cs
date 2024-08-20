namespace RealmCore.Persistence.Data;

public sealed class TimeBaseOperationData
{
    public int Id { get; set; }
    public int Type { get; set; }
    public int Status { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }

    public TimeBaseOperationDataGroupUserData? TimeBasedOperationDataGroupUser { get; set; }
}

public sealed class TimeBaseOperationGroupData
{
    public int Id { get; set; }
    public int Category { get; set; }
    public int Limit { get; set; }
    public string? Metadata { get; set; }

    public ICollection<TimeBaseOperationDataGroupUserData>? TimeBasedOperationDataGroupUser { get; set; }
}

public sealed class TimeBaseOperationDataGroupUserData
{
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public int TimeBasedOperationId { get; set; }
    public string? Metadata { get; set; }

    public TimeBaseOperationGroupData? Group { get; set; }
    public TimeBaseOperationData? Operation { get; set; }
    public UserData User { get; set; }
}
