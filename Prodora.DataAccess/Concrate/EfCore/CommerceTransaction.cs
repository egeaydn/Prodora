using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

internal static class CommerceTransaction
{
    // SQL Server application locks coordinate basket/checkout writes across app instances.
    public static IDbContextTransaction Begin(DataContext context, string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var resource = "Prodora:basket:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId)));
            context.Database.ExecuteSqlInterpolated($@"DECLARE @result int;
                EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
                IF @result < 0 THROW 51000, 'Basket lock unavailable', 1;");
            return transaction;
        }
        catch { transaction.Dispose(); throw; }
    }

    public static void EnsureEditable(DataContext context, string userId)
    {
        if (context.Orders.Any(o => o.UserId == userId &&
            (o.OrderEnums == OrderStatus.PaymentPending || o.OrderEnums == OrderStatus.PaymentUncertain)))
            throw new StoreValidationException("Son ödeme girişimin henüz sonuçlanmadı. Siparişlerim sayfasından durumunu kontrol et.");
    }
}
