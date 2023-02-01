namespace Realm.Persistance.Interfaces;

public interface IFractionRepository
{
    Task<FractionMember> CreateNewFractionMember(int fractionId, Guid userId, int rank = 1, string rankName = "");
}
