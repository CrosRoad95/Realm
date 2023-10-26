

namespace RealmCore.Persistence.Interfaces;

public interface IBanRepository
{
    Task<BanData> CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default);
    Task<BanData> CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default);
    Task<BanData> CreateBanForUserIdAndSerial(int userId, string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default);
    Task<bool> Delete(int id, CancellationToken cancellationToken = default);
    Task<List<int>> DeleteBySerial(string serial, int? type = 0, CancellationToken cancellationToken = default);
    Task<List<int>> DeleteByUserId(int userId, int? type = 0, CancellationToken cancellationToken = default);
    Task<List<int>> DeleteByUserIdOrSerial(int userId, string serial, int? type = 0, CancellationToken cancellationToken = default);
    Task<List<BanData>> GetBansBySerial(string serial, DateTime now, int? type = null, CancellationToken cancellationToken = default);
    Task<List<BanData>> GetBansByUserId(int userId, DateTime now, int? type = null, CancellationToken cancellationToken = default);
    Task<List<BanData>> GetBansByUserIdOrSerial(int userId, string serial, DateTime now, int? type = null, CancellationToken cancellationToken = default);
}
