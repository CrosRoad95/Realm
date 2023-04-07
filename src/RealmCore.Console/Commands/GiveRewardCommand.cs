namespace RealmCore.Console.Commands;


[CommandName("givereward")]
public sealed class GiveRewardCommand : IIngameCommand
{
    private readonly ILogger<GiveRewardCommand> _logger;
    private readonly IRewardService _rewardService;

    public GiveRewardCommand(ILogger<GiveRewardCommand> logger, IRewardService rewardService)
    {
        _logger = logger;
        _rewardService = rewardService;
    }

    public async Task Handle(Entity entity, string[] args)
    {
        if (await _rewardService.TryGiveReward(entity, 1))
        {
            entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Nagroda id 1 została odebrana pomyślnie");
        }
        else
        {
            entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage("Już otrzymałeś nagrode id 1");
        }
    }
}