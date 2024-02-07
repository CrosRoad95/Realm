namespace RealmCore.Server.Rules;

public sealed class MustHaveNoWorldObjectAttachedRule : IElementRule
{
    public bool Check(Element element)
    {
        if(element is RealmPlayer player)
        {
            return player.AttachedBoneElementsCount == 0;
        }
        return false;
    }
}
