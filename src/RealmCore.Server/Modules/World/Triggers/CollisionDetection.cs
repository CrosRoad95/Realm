namespace RealmCore.Server.Modules.World.Triggers;

public abstract class CollisionDetection
{
    private readonly object _lock = new();

    private readonly List<IElementRule> _elementRules = [];
    private readonly IServiceProvider _serviceProvider;

    public CollisionDetection(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void AddRule(IElementRule elementRule)
    {
        lock (_lock)
            _elementRules.Add(elementRule);
    }

    public void AddRule<TElementRule>() where TElementRule : IElementRule, new()
    {
        lock (_lock)
            _elementRules.Add(new TElementRule());
    }

    public void AddRuleWithDI<TElementRule>(params object[] parameters) where TElementRule : IElementRule
    {
        lock (_lock)
            _elementRules.Add(ActivatorUtilities.CreateInstance<TElementRule>(_serviceProvider, parameters));
    }

    public bool CheckRules(Element element)
    {
        lock (_lock)
            foreach (var rule in _elementRules)
            {
                if (!rule.Check(element))
                    return false;
            }
        return true;
    }

    internal abstract void RelayEntered(Element element);
    internal abstract void RelayLeft(Element element);
}

public class CollisionDetection<T> : CollisionDetection
{
    private readonly T _that;

    public event Action<T, Element>? Entered;
    public event Action<T, Element>? Left;

    public CollisionDetection(IServiceProvider serviceProvider, T that) : base(serviceProvider)
    {
        _that = that;
    }

    internal override void RelayEntered(Element element)
    {
        Entered?.Invoke(_that, element);
    }

    internal override void RelayLeft(Element element)
    {
        Left?.Invoke(_that, element);
    }
}
