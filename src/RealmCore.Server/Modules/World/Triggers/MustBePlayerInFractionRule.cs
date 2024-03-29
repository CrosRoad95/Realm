﻿namespace RealmCore.Server.Modules.World.Triggers;

public sealed class MustBePlayerInFractionRule : IElementRule
{
    private readonly int _fractionId;

    public MustBePlayerInFractionRule(int fractionId)
    {
        _fractionId = fractionId;
    }

    public bool Check(Element element)
    {
        if (element is RealmPlayer player)
        {
            return player.Fractions.IsMember(_fractionId);
        }
        return false;
    }
}
