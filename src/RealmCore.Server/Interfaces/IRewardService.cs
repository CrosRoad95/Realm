using RealmCore.ECS;

namespace RealmCore.Server.Interfaces;

public interface IRewardService
{
    Task<bool> TryGiveReward(Entity entity, int rewardId);
}
