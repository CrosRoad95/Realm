﻿using Realm.Persistance.Interfaces;

namespace Realm.Server.Services;

internal class RewardService : IRewardService
{
    private readonly IUserRewardRepository _userRewardRepository;

    public RewardService(IUserRewardRepository userRewardRepository)
    {
        _userRewardRepository = userRewardRepository;
    }

    public async Task<bool> TryGiveReward(Entity entity, int rewardId)
    {
        return await _userRewardRepository.TryAddReward(entity.GetRequiredComponent<AccountComponent>().Id, rewardId);
    }
}