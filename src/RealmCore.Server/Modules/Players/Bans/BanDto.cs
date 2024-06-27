namespace RealmCore.Server.Modules.Players.Bans;

public sealed class BanDto : IEqualityComparer<BanDto>
{
    public required int Id { get; init; }
    public required DateTime End { get; init; }
    public required int? UserId { get; init; }
    public required string? Serial { get; init; }
    public required string? Reason { get; init; }
    public required string? Responsible { get; init; }
    public required int Type { get; init; }
    public required bool Active { get; init; }

    public bool Equals(BanDto? x, BanDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] BanDto obj) => obj.Id;

    public bool IsActive(DateTime now) => Active && End > now;

    [return: NotNullIfNotNull(nameof(banData))]
    public static BanDto? Map(BanData? banData)
    {
        if (banData == null)
            return null;

        return new()
        {
            Id = banData.Id,
            End = banData.End,
            UserId = banData.UserId,
            Reason = banData.Reason,
            Responsible = banData.Responsible,
            Serial = banData.Serial,
            Type = banData.Type,
            Active = banData.Active,
        };
    }
}
