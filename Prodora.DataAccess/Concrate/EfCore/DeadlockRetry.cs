using Microsoft.Data.SqlClient;

namespace Prodora.DataAccess.Concrate.EfCore;

/// <summary>
/// Concurrent checkouts under RepeatableRead can pick one transaction as a deadlock victim (SQL error 1205).
/// The victim is rolled back completely, so running the whole unit of work again is safe.
/// </summary>
internal static class DeadlockRetry
{
    public static T Run<T>(Func<T> unitOfWork, int attempts = 4)
    {
        for (var attempt = 1; ; attempt++)
        {
            try { return unitOfWork(); }
            catch (Exception exception) when (attempt < attempts && IsDeadlock(exception))
            {
                Thread.Sleep(20 * attempt + Random.Shared.Next(30));
            }
        }
    }

    public static void Run(Action unitOfWork) => Run(() => { unitOfWork(); return true; });

    private static bool IsDeadlock(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
            if (current is SqlException { Number: 1205 }) return true;
        return false;
    }
}
