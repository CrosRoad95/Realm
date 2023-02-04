namespace Realm.Persistance.Interfaces;

public interface IFractionRepository
{
    Task<FractionMember> CreateNewFractionMember(int fractionId, int UserId, int rank = 1, string rankName = "");
}
