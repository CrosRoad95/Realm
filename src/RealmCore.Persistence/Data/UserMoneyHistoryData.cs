namespace RealmCore.Persistence.Data;

public class UserMoneyHistoryData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime DateTime { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? Description { get; set; }
}
