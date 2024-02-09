namespace RealmCore.Server.Modules.Players.Bans;

public class BanDto : IEqualityComparer<BanDto>
{
    public int Id { get; init; }
    public DateTime End { get; init; }
    public int? UserId { get; init; }
    public string? Serial { get; init; }
    public string? Reason { get; init; }
    public string? Responsible { get; init; }
    public int Type { get; init; }

    public bool Equals(BanDto? x, BanDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] BanDto obj) => obj.Id;

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
            Type = banData.Type
        };
    }
}
