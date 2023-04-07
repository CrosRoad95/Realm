namespace Realm.Persistance.Interfaces;

public interface IFractionRepository : IRepositoryBase
{
    void CreateFraction(int id, string fractionName, string fractionCode);
    void AddFractionMember(int fractionId, int userId, int rank = 1, string rankName = "");
    Task<bool> Exists(int id, string code, string name);
    Task<List<FractionMemberData>> GetAllMembers(int fractionId);
}
