namespace RealmCore.Persistence.DTOs;

public sealed class UserJobStatisticsDto
{
    public required int UserId { get; init; }
    public required int JobId { get; init; }
    public required ulong Points { get; init; }
    public required ulong TimePlayed { get; init; }
}
