namespace RealmCore.Persistence.Interfaces;

public interface IFractionRepository
{
    Task<bool> CreateFraction(int id, string fractionName, string fractionCode, CancellationToken cancellationToken = default);
    Task<bool> Exists(int id, string code, string name, CancellationToken cancellationToken = default);
    Task<List<FractionData>> GetAll(CancellationToken cancellationToken = default);
    Task<List<FractionMemberData>> GetAllMembers(int fractionId, CancellationToken cancellationToken = default);
    Task<FractionMemberData?> TryAddMember(int fractionId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default);
    Task<FractionData?> TryCreateFraction(int id, string fractionName, string fractionCode, CancellationToken cancellationToken = default);
}
