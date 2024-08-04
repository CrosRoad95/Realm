namespace RealmCore.Persistence.Data.Bases;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class EventDataBase
{
    public int Id { get; set; }
    public int EventType { get; set; }
    public string? Metadata { get; set; }
    public DateTime DateTime { get; set; }
    private string DebuggerDisplay
    {
        get
        {
            if(Metadata != null)
            {
                return $"At {DateTime}, EventType={EventType} {Metadata}";
            }
            else
            {
                return $"At {DateTime}, EventType={EventType}";
            }
        }
    }
}
