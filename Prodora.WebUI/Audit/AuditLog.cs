using System.Security.Claims;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.WebUI.Audit;

public interface IAuditLog
{
    /// <summary>Records an admin action for the signed-in user. It never throws: a failed write must not undo or hide the action itself.</summary>
    void Record(string action, string target, string? detail = null);
}

public sealed class AuditLog(IAuditDal store, IHttpContextAccessor accessor, ILogger<AuditLog> logger) : IAuditLog
{
    public void Record(string action, string target, string? detail = null)
    {
        try
        {
            var http = accessor.HttpContext;
            var user = http?.User;
            store.Add(new AuditEntry
            {
                At = DateTime.Now,
                ActorId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                ActorName = user?.Identity?.Name ?? "",
                Action = action, Target = target, Detail = detail ?? "",
                Ip = http?.Connection.RemoteIpAddress?.ToString() ?? ""
            });
        }
        catch (Exception exception)
        {
            // The action already happened; make the gap visible in the application log instead of failing the request.
            logger.LogError("Audit entry {Action} for {Target} could not be written; error type {ErrorType}", action, target, exception.GetType().Name);
        }
    }
}
