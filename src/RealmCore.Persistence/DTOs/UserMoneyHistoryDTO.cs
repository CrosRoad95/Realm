using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Persistence.DTOs;

public class UserMoneyHistoryDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public decimal Amount { get; set; }
    public decimal CurrentBalance { get; set; }
    public int? Category { get; set; }
    public string? Description { get; set; }

    [return: NotNullIfNotNull(nameof(userMoneyHistoryData))]
    public static UserMoneyHistoryDto? Map(UserMoneyHistoryData? userMoneyHistoryData)
    {
        if (userMoneyHistoryData == null)
            return null;

        return new UserMoneyHistoryDto
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
