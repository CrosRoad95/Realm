namespace RealmCore.Server.Interfaces;

public interface IFractionService
{
    bool Exists(int id);
    bool HasMember(int fractionId, int userId);
    internal Task<bool> TryCreateFraction(int id, string fractionName, string fractionCode, Vector3 position, CancellationToken cancellationToken = default);
    internal Task<bool> InternalExists(int id, string code, string name, CancellationToken cancellationToken = default);
    Task LoadFractions(CancellationToken cancellationToken);
    Task<bool> TryAddMember(int fractionId, int userId, int rank, string rankName);
}
