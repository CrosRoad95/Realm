namespace RealmCore.Persistence.Data;

public sealed class TimeBaseOperationData
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int Type { get; set; }
    public int Status { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? Metadata { get; set; }

    public TimeBaseOperationGroupData? Group { get; set; }
}

public sealed class TimeBaseOperationGroupData
{
    public int Id { get; set; }
    public int Category { get; set; }
    public int Limit { get; set; }
    public string? Metadata { get; set; }

    public ICollection<TimeBaseOperationData>? Operations { get; set; }
    public ICollection<TimeBaseOperationGroupUserData>? GroupUserOperations { get; set; }
}

public sealed class TimeBaseOperationGroupUserData
{
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public string? Metadata { get; set; }

    public TimeBaseOperationGroupData? Group { get; set; }
    public UserData User { get; set; }
}
