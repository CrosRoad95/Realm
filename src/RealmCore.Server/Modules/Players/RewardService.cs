﻿namespace RealmCore.Server.Modules.Players;

public interface IRewardService
{
    Task<bool> TryGiveReward(RealmPlayer player, int rewardId, CancellationToken cancellationToken = default);
}

internal sealed class RewardService : IRewardService
{
    private readonly IUserRewardRepository _userRewardRepository;

    public RewardService(IUserRewardRepository userRewardRepository)
    {
        _userRewardRepository = userRewardRepository;
    }

    public async Task<bool> TryGiveReward(RealmPlayer player, int rewardId, CancellationToken cancellationToken = default)
    {
        return await _userRewardRepository.TryAddReward(player.PersistentId, rewardId, cancellationToken);
    }
}
