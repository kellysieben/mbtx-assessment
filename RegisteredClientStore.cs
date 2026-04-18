using System.Collections.Concurrent;

namespace MbtxAssessment.DataStore;

public sealed class RegisteredClientStore : IRegisteredClientStore
{
    private readonly ConcurrentDictionary<string, byte> _registeredClients =
        new(StringComparer.OrdinalIgnoreCase);

    public bool RegisterClient(string clientId)
    {
        return _registeredClients.TryAdd(clientId.Trim(), 0);
    }

    public bool IsRegistered(string clientId)
    {
        return _registeredClients.ContainsKey(clientId.Trim());
    }

    public IReadOnlyCollection<string> GetRegisteredClients()
    {
        return _registeredClients.Keys.ToArray();
    }
}