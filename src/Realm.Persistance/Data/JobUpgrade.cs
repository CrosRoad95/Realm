namespace Realm.Persistance.Data;

public class JobUpgrade
{
    public Guid UserId { get; set; }
    public short JobId { get; set; }
    public string Name { get; set; }

    public virtual User User { get; set; }
}
