namespace Realm.Persistance.Data;

public class Achievement
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public float Progress { get; set; }
    public string? Value { get; set; }
    public bool PrizeReceived { get; set; }

    public User User { get; set; }
}
