namespace RealmCore.Persistence.Interfaces;

public interface IFractionRepository
{
    Task<bool> AddMember(int fractionId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default);
    Task<bool> CreateFraction(int id, string fractionName, string fractionCode, CancellationToken cancellationToken = default);
    Task<bool> Exists(int id, string code, string name, CancellationToken cancellationToken = default);
    Task<List<FractionMemberData>> GetAllMembers(int fractionId, CancellationToken cancellationToken = default);
}
