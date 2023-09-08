namespace RealmCore.Persistence.Interfaces;

public interface IBanRepository
{
    Task<BanData> CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default);
    Task<BanData> CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default);
    Task<List<BanData>> GetBansByUserId(int userId, DateTime now, CancellationToken cancellationToken = default);
    Task<List<BanData>> GetBansBySerial(string serial, DateTime now, CancellationToken cancellationToken = default);
    Task<bool> Delete(int banId, CancellationToken cancellationToken = default);
    Task<bool> DeleteByUserId(int userId, int type = 0, CancellationToken cancellationToken = default);
    Task<bool> DeleteBySerial(string serial, int type = 0, CancellationToken cancellationToken = default);
    Task<List<BanData>> GetBansByUserIdOrSerial(int userId, string serial, DateTime now, CancellationToken cancellationToken = default);
    Task<BanData?> GetBanBySerialAndType(string serial, int type, DateTime now, CancellationToken cancellationToken = default);
}
