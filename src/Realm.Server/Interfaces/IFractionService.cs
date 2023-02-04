namespace Realm.Server.Interfaces;

public interface IFractionService
{
    Task AddMember(int fractionId, int UserId, int rank, string rankName);
    void CreateFraction(int id, string fractionName, string fractionCode, Vector3 position);
    internal void InternalAddMember(int fractionId, int UserId, int rank, string rankName);
}
