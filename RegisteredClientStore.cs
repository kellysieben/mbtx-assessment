using Microsoft.EntityFrameworkCore;

namespace MbtxAssessment.DataStore;

public sealed class RegisteredClientStore(IDbContextFactory<AppDbContext> dbContextFactory) : IRegisteredClientStore
{
    public bool RegisterClient(string clientId)
    {
        using var db = dbContextFactory.CreateDbContext();
        var trimmed = clientId.Trim();
        if (db.RegisteredClients.Any(c => c.ClientId == trimmed))
            return false;

        db.RegisteredClients.Add(new RegisteredClientEntity { ClientId = trimmed, RegisteredAt = DateTime.Now });
        db.SaveChanges();
        return true;
    }

    public bool IsRegistered(string clientId)
    {
        using var db = dbContextFactory.CreateDbContext();
        return db.RegisteredClients.Any(c => c.ClientId == clientId.Trim());
    }

    public IReadOnlyCollection<string> GetRegisteredClients()
    {
        using var db = dbContextFactory.CreateDbContext();
        return db.RegisteredClients.Select(c => c.ClientId).ToArray();
    }
}