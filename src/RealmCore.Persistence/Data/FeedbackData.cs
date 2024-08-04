namespace RealmCore.Persistence.Data;

public class OpinionData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OpinionId { get; set; }
    public string Opinion { get; set; }
    public DateTime DateTime { get; set; }

    public UserData? User { get; set; }
}

public class RatingData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RatingId { get; set; }
    public int Rating { get; set; }
    public DateTime DateTime { get; set; }

    public UserData? User { get; set; }
}
