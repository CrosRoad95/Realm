namespace RealmCore.Server.Logic.Elements;

internal sealed class PickupLogic
{
    private readonly IElementFactory _elementFactory;
    private readonly ILogger<PickupLogic> _logger;

    public PickupLogic(IElementFactory elementFactory, ILogger<PickupLogic> logger)
    {
        _elementFactory = elementFactory;
        _logger = logger;
        _elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(Element element)
    {
        if(element is not RealmPickup pickup)
            return;

        void HandleElementEntered(Element element)
        {
            try
            {
                if (pickup.CollisionDetection.CheckRules(element))
                {
                    pickup.CollisionDetection.RelayEntered(element);
                }
            }
            catch(Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }

        void HandleElementLeft(Element element)
        {
            try
            {
                pickup.CollisionDetection.RelayLeft(element);
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }

        void HandleDestroyed(Element element)
        {
            var pickup = (RealmPickup)element;
            pickup.CollisionShape.ElementEntered -= HandleElementEntered;
            pickup.CollisionShape.ElementLeft -= HandleElementLeft;
            pickup.Destroyed -= HandleDestroyed;
        }

        pickup.CollisionShape.ElementEntered += HandleElementEntered;
        pickup.CollisionShape.ElementLeft += HandleElementLeft;
        pickup.Destroyed += HandleDestroyed;
    }
}
