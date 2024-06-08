using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Persistence.Dto;

public sealed class UserMoneyHistoryDto
{
    public required int Id { get; init; }
    public required DateTime DateTime { get; init; }
    public required decimal Amount { get; init; }
    public required decimal CurrentBalance { get; init; }
    public required int? Category { get; init; }
    public required string? Description { get; init; }

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
