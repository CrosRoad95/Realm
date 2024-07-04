namespace RealmCore.Persistence.Data;

public class WorldNodeData
{
    public int Id { get; set; }
    public string TypeName { get; set; }
    public TransformData Transform { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public string? MetaData { get; set; }

    public ICollection<WorldNodeScheduledActionData> ScheduledActionData { get; set; } = [];
}

public class WorldNodeScheduledActionData
{
    public int Id { get; set; }
    public int WorldNodeId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string ActionData { get; set; }

    public WorldNodeData?WorldNode { get; set; }
}