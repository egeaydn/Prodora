using Prodora.Entitys;

namespace Prodora.DataAccess.Abstract;

public interface IAuditDal
{
    void Add(AuditEntry entry);
    /// <summary>Newest first. <paramref name="actionPrefix"/> narrows to one area, e.g. "order".</summary>
    List<AuditEntry> Query(string? actionPrefix, int page, int pageSize);
    int Count(string? actionPrefix);
}
