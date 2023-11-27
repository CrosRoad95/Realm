namespace RealmCore.Server.DTOs;

public class BanDTO : IEqualityComparer<BanDTO>
{
    public int Id { get; set; }
    public DateTime End { get; set; }
    public int? UserId { get; set; }
    public string? Serial { get; set; }
    public string? Reason { get; set; }
    public string? Responsible { get; set; }
    public int Type { get; set; }

    public bool Equals(BanDTO? x, BanDTO? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] BanDTO obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(banData))]
    public static BanDTO? Map(BanData? banData)
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
