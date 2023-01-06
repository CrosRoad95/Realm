namespace Realm.Persistance.Data;

public class JobUpgrade
{
    public Guid UserId { get; set; }
    public short JobId { get; set; }
    public string Name { get; set; }

    public User User { get; set; }
}
