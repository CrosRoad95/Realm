namespace RealmCore.Persistance.Data;

public class OpinionData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OpinionId { get; set; }
    public string Opinion { get; set; }
    public DateTime DateTime { get; set; }

    public UserData? User { get; set; }
}
