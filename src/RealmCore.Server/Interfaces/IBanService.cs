
namespace RealmCore.Server.Interfaces;

public interface IBanService
{
    Task Ban(Entity entity, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task BanAccount(Entity entity, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task BanPlayer(Entity entity, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0);
    Task<List<BanData>> GetBans(Entity entity);
    Task<bool> IsBanned(Entity entity, int? type = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveBan(Entity entity, int type = 0);
}
