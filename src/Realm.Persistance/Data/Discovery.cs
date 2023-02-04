namespace Realm.Persistance.Data;

public class Discovery
{
    public int UserId { get; set; }
    public string DiscoveryId { get; set; }

    public virtual User? User { get; set; }
}
