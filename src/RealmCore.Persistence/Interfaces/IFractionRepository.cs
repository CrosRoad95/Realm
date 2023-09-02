namespace RealmCore.Persistence.Interfaces;

public interface IFractionRepository
{
    Task<bool> AddMember(int fractionId, int userId, int rank = 1, string rankName = "");
    Task<bool> CreateFraction(int id, string fractionName, string fractionCode);
    Task<bool> Exists(int id, string code, string name);
    Task<List<FractionMemberData>> GetAllMembers(int fractionId);
}
