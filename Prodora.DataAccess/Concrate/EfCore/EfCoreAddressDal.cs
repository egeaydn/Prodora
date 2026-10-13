using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Prodora.DataAccess.Abstract;
using Prodora.Entitys;

namespace Prodora.DataAccess.Concrate.EfCore;

public class EfCoreAddressDal(IDbContextFactory<DataContext> factory) : IAddressDal
{
    public List<SavedAddress> GetAll(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return new();
        using var context = factory.CreateDbContext();
        return context.SavedAddresses.AsNoTracking().Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id).ToList();
    }

    public SavedAddress? Get(string userId, int id)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;
        using var context = factory.CreateDbContext();
        return context.SavedAddresses.AsNoTracking().FirstOrDefault(a => a.UserId == userId && a.Id == id);
    }

    public SavedAddress Add(string userId, SavedAddress address)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new StoreValidationException("Adres kaydetmek için giriş yap.");
        AddressRules.Normalize(address);
        var error = AddressRules.Validate(address);
        if (error != null) throw new StoreValidationException(error);
        return DeadlockRetry.Run(() =>
        {
            using var context = factory.CreateDbContext();
            using var transaction = Begin(context, userId);
            var mine = context.SavedAddresses.Where(a => a.UserId == userId).ToList();
            var duplicate = mine.FirstOrDefault(a => a.Address == address.Address && a.City == address.City && a.Phone == address.Phone
                && a.FirstName == address.FirstName && a.LastName == address.LastName);
            if (duplicate != null) return duplicate;
            if (mine.Count >= AddressRules.MaxPerUser)
                throw new StoreValidationException($"Adres defterinde en fazla {AddressRules.MaxPerUser} adres tutabilirsin. Önce birini sil.");
            address.Id = 0;
            address.UserId = userId;
            address.CreatedAt = DateTime.Now;
            address.IsDefault = mine.Count == 0;
            context.SavedAddresses.Add(address);
            context.SaveChanges();
            transaction.Commit();
            return address;
        });
    }

    public void SetDefault(string userId, int id) => DeadlockRetry.Run(() =>
    {
        using var context = factory.CreateDbContext();
        using var transaction = Begin(context, userId);
        var mine = context.SavedAddresses.Where(a => a.UserId == userId).ToList();
        var target = mine.FirstOrDefault(a => a.Id == id) ?? throw new StoreValidationException("Adres bulunamadı.");
        // Two saves: the unique index allows only one default, so the old one must be cleared first.
        foreach (var other in mine.Where(a => a.IsDefault && a.Id != id)) other.IsDefault = false;
        context.SaveChanges();
        target.IsDefault = true;
        context.SaveChanges();
        transaction.Commit();
    });

    public void Delete(string userId, int id) => DeadlockRetry.Run(() =>
    {
        using var context = factory.CreateDbContext();
        using var transaction = Begin(context, userId);
        var mine = context.SavedAddresses.Where(a => a.UserId == userId).ToList();
        var target = mine.FirstOrDefault(a => a.Id == id) ?? throw new StoreValidationException("Adres bulunamadı.");
        context.SavedAddresses.Remove(target);
        context.SaveChanges();
        if (target.IsDefault)
        {
            var next = mine.Where(a => a.Id != id).OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id).FirstOrDefault();
            if (next != null) { next.IsDefault = true; context.SaveChanges(); }
        }
        transaction.Commit();
    });

    // One address change per customer at a time, so two quick requests cannot both claim "first address" or "default".
    private static IDbContextTransaction Begin(DataContext context, string userId)
    {
        var transaction = context.Database.BeginTransaction();
        try
        {
            var resource = "Prodora:address:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(userId)));
            context.Database.ExecuteSqlInterpolated($@"DECLARE @result int;
                EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
                IF @result < 0 THROW 51000, 'Address lock unavailable', 1;");
            return transaction;
        }
        catch { transaction.Dispose(); throw; }
    }
}
