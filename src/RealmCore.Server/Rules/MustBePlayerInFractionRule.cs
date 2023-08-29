namespace RealmCore.Server.Rules;

public sealed class MustBePlayerInFractionRule : IEntityRule
{
    private readonly int _fractionId;

    public MustBePlayerInFractionRule(int fractionId)
    {
        _fractionId = fractionId;
    }

    public bool Check(Entity entity)
    {
        if (!entity.HasComponent<PlayerTagComponent>())
            return false;

        return entity.Components.OfType<FractionMemberComponent>().Where(x => x.FractionId == _fractionId).Any();
    }
}
