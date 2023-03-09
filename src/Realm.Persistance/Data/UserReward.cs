namespace Realm.Persistance.Data;

public class UserReward
{
    public int UserId { get; set; }
    public int RewardId { get; set; }

    public User? User { get; set; }
}
