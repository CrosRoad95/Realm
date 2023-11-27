namespace RealmCore.Server.Interfaces;

public interface IFractionService
{
    Task<bool> TryAddMember(int fractionId, int userId, int rank, string rankName, CancellationToken cancellationToken = default);
    bool Exists(int id);
    bool HasMember(int fractionId, int userId);
    internal Task<bool> InternalCreateFraction(int id, string fractionName, string fractionCode, Vector3 position, CancellationToken cancellationToken = default);
    internal Task<bool> InternalExists(int id, string code, string name, CancellationToken cancellationToken = default);
}
