using RealmCore.Resources.Overlay.ConstructionInfos;

namespace RealmCore.Resources.Overlay;

internal class TextAndHudBuilder<TState> : ITextAndHudBuilder<TState>
{
    private readonly HudBuilder<TState> _innerHudBuilder;
    private readonly TextConstructionInfo _textConstructionInfo;

    public TextAndHudBuilder(HudBuilder<TState> innerHudBuilder, TextConstructionInfo textConstructionInfo)
    {
        _innerHudBuilder = innerHudBuilder;
        _textConstructionInfo = textConstructionInfo;
    }

    public void AddShadow(Vector2 offset)
    {
        var constructionInfo = _textConstructionInfo with
        {
            id = _innerHudBuilder.AllocateId(),
            color = Color.Black,
            position = _textConstructionInfo.position + offset
        };

        if(_innerHudBuilder.DynamicHudComponentAdded != null)
        {
            foreach (var propertyInfo in constructionInfo.propertyInfos)
            {
                _innerHudBuilder.DynamicHudComponentAdded.Invoke(new DynamicHudComponent
                {
                    ComponentId = constructionInfo.id,
                    PropertyInfo = propertyInfo
                });
            }
        }
        _innerHudBuilder.AddLuaValue(constructionInfo.AsLuaValue(), true);
    }
}
