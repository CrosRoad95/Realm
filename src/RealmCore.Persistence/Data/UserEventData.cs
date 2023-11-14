using System.Diagnostics;

namespace RealmCore.Persistence.Data;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class UserEventData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventType { get; set; }
    public string? Metadata { get; set; }
    public DateTime DateTime { get; set; }
    private string DebuggerDisplay
    {
        get
        {
            var metaData = Metadata != null ? $" MetaData {Metadata}" : "";
            return $"At {DateTime}, EventType={EventType} {metaData}";
        }
    }
}
