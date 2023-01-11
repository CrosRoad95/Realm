namespace Realm.Persistance.Data;

public class Discovery
{
    public Guid UserId { get; set; }
    public string DiscoveryId { get; set; }

    public virtual User? User { get; set; }
}
