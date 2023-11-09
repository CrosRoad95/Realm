
namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerLicensesService : IPlayerService, IEnumerable<LicenseDTO>
{
    event Action<IPlayerLicensesService, int>? Added;
    event Action<IPlayerLicensesService, int, DateTime, string?>? Suspended;
    event Action<IPlayerLicensesService, int>? UnSuspended;

    LicenseDTO? Get(int licenseId);
    bool Has(int licenseId, bool includeSuspended = false);
    bool IsSuspended(int licenseId);
    void Suspend(int licenseId, TimeSpan timeSpan, string? reason = null);
    bool TryAdd(int licenseId);
    void UnSuspend(int licenseId);
}
