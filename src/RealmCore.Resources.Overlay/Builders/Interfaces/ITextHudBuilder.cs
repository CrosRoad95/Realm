﻿using RealmCore.Resources.Overlay.ConstructionInfos;

namespace RealmCore.Resources.Overlay.Builders.Interfaces;

public interface ITextHudBuilder<TState>
{
    ITextHudBuilder<TState> WithColor(Color color);
    ITextHudBuilder<TState> WithFont(IFont font);
    ITextHudBuilder<TState> WithFont(string stringFont);
    ITextHudBuilder<TState> WithHorizontalAlign(HorizontalAlign alignX);
    ITextHudBuilder<TState> WithPosition(Vector2 position);
    ITextHudBuilder<TState> WithScale(Size? scale);
    ITextHudBuilder<TState> WithSize(Size size);
    ITextHudBuilder<TState> WithText(string text);
    ITextHudBuilder<TState> WithText(Expression<Func<TState, string>> expression);
    ITextHudBuilder<TState> WithVerticalAlign(VerticalAlign alignY);
    internal Action<DynamicHudElement>? DynamicHudElementAdded { get; set; }
    bool IsDynamic { get; }

    internal TextConstructionInfo Build();
    ITextHudBuilder<TState> WithComputedValue(ComputedValueType computedValueType);
}
