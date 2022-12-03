namespace Realm.Persistance.Logger.Enrichers;

internal class PlayerAccountEnricher : ILogEventEnricher
{
    private readonly PlayerAccount _playerAccount;

    public PlayerAccountEnricher(PlayerAccount playerAccount)
    {
        _playerAccount = playerAccount;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("accountId", _playerAccount.Id));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("accountName", _playerAccount.ToString()));
    }
}
