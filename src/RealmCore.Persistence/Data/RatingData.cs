namespace RealmCore.Persistence.Data;

public class RatingData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RatingId { get; set; }
    public int Rating { get; set; }
    public DateTime DateTime { get; set; }

    public UserData? User { get; set; }
}
