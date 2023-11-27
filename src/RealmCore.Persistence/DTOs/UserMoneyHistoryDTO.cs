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

    [return: NotNullIfNotNull(nameof(aserMoneyHistoryData))]
    public static UserMoneyHistoryDTO? Map(UserMoneyHistoryData? aserMoneyHistoryData)
    {
        if (aserMoneyHistoryData == null)
            return null;

        return new UserMoneyHistoryDTO
        {
            Id = aserMoneyHistoryData.Id,
            DateTime = aserMoneyHistoryData.DateTime,
            Amount = aserMoneyHistoryData.Amount,
            CurrentBalance = aserMoneyHistoryData.CurrentBalance,
            Category = aserMoneyHistoryData.Category,
            Description = aserMoneyHistoryData.Description
        };
    }
}
