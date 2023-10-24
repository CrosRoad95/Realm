namespace RealmCore.Persistence.DTOs;

public class UserMoneyHistoryDTO
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public int? Category { get; set; }
    public string? Description { get; set; }
}
