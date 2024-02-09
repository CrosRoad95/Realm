using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Persistence.DTOs;

public class UserMoneyHistoryDTO
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public int? Category { get; set; }
    public string? Description { get; set; }

    [return: NotNullIfNotNull(nameof(userMoneyHistoryData))]
    public static UserMoneyHistoryDTO? Map(UserMoneyHistoryData? userMoneyHistoryData)
    {
        if (userMoneyHistoryData == null)
            return null;

        return new UserMoneyHistoryDTO
        {
            Id = userMoneyHistoryData.Id,
            DateTime = userMoneyHistoryData.DateTime,
            Amount = userMoneyHistoryData.Amount,
            CurrentBalance = userMoneyHistoryData.CurrentBalance,
            Category = userMoneyHistoryData.Category,
            Description = userMoneyHistoryData.Description
        };
    }
}
