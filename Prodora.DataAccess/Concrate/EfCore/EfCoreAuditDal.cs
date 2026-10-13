using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

public class EfCoreAuditDal(IDbContextFactory<DataContext> factory) : IAuditDal
{
    public void Add(AuditEntry entry)
    {
        entry.Id = 0;
        entry.Action = AuditEntry.Clean(entry.Action, AuditEntry.ActionLength);
        entry.Target = AuditEntry.Clean(entry.Target, AuditEntry.TargetLength);
        entry.Detail = AuditEntry.Clean(entry.Detail, AuditEntry.DetailLength);
        entry.ActorName = AuditEntry.Clean(entry.ActorName, AuditEntry.ActorNameLength);
        entry.Ip = AuditEntry.Clean(entry.Ip, AuditEntry.IpLength);
        using var context = factory.CreateDbContext();
        context.AuditEntries.Add(entry);
        context.SaveChanges();
    }

    public List<AuditEntry> Query(string? actionPrefix, int page, int pageSize)
    {
        using var context = factory.CreateDbContext();
        return Filter(context, actionPrefix).OrderByDescending(e => e.At).ThenByDescending(e => e.Id)
            .Skip((Math.Max(page, 1) - 1) * pageSize).Take(pageSize).ToList();
    }

    public int Count(string? actionPrefix)
    {
        using var context = factory.CreateDbContext();
        return Filter(context, actionPrefix).Count();
    }

    private static IQueryable<AuditEntry> Filter(DataContext context, string? actionPrefix)
    {
        var entries = context.AuditEntries.AsNoTracking();
        var prefix = AuditEntry.Clean(actionPrefix, AuditEntry.ActionLength);
        // EF escapes '%' and '_' in a StartsWith argument, so the prefix cannot act as a wildcard pattern.
        return prefix.Length == 0 ? entries : entries.Where(e => e.Action.StartsWith(prefix));
    }
}
