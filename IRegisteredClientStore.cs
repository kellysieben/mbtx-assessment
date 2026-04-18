namespace MbtxAssessment.DataStore;

public interface IRegisteredClientStore
{
    bool RegisterClient(string clientId);
    bool IsRegistered(string clientId);
    IReadOnlyCollection<string> GetRegisteredClients();
}
