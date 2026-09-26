using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Prodora.DataAccess.Concrate.EfCore;

internal static class DatabaseMaintenance
{
    private const string Migration = "20260926170409_PreserveOrdersAndCheckout";
    public static async Task Run(string connectionString, bool apply)
    {
        await using var context = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlServer(connectionString).Options);
        var pending = (await context.Database.GetPendingMigrationsAsync()).ToArray();
        Console.WriteLine("Pending commerce migrations: " + string.Join(", ", pending));
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        async Task<string> Counts()
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT (SELECT COUNT_BIG(*) FROM ProdoraProducts), (SELECT COUNT_BIG(*) FROM Orders), (SELECT COUNT_BIG(*) FROM OrderItem), (SELECT COUNT_BIG(*) FROM Baskets), (SELECT COUNT_BIG(*) FROM BasketItem), (SELECT COUNT_BIG(*) FROM Comments)";
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            return string.Join(",", Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetInt64(i)));
        }
        var before = await Counts();
        Console.WriteLine("Counts (products,orders,order-items,baskets,basket-items,comments): " + before);
        if (!apply || pending.Length == 0) return;
        if (pending.Length != 1 || pending[0] != Migration) throw new InvalidOperationException("Unexpected migration history; refusing automatic update.");
        await using var backup = connection.CreateCommand();
        backup.CommandTimeout = 180;
        backup.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000))";
        var backupDirectory = (string?)await backup.ExecuteScalarAsync();
        if (string.IsNullOrWhiteSpace(backupDirectory)) throw new InvalidOperationException("SQL Server backup directory is not configured.");
        var database = connection.Database;
        var backupPath = Path.Combine(backupDirectory, "Prodora_before_reliability_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N")[..8] + ".bak");
        backup.CommandText = "BACKUP DATABASE " + new SqlCommandBuilder().QuoteIdentifier(database) + " TO DISK=@path WITH COPY_ONLY, CHECKSUM";
        backup.Parameters.AddWithValue("@path", backupPath);
        await backup.ExecuteNonQueryAsync();
        backup.CommandText = "RESTORE VERIFYONLY FROM DISK=@path WITH CHECKSUM";
        await backup.ExecuteNonQueryAsync();
        Console.WriteLine("Verified backup: " + backupPath);
        await context.Database.MigrateAsync();
        var after = await Counts();
        if (before != after) throw new InvalidOperationException("Record counts changed; investigate before continuing.");
        Console.WriteLine("Migration applied; all six record counts unchanged: " + after);
    }
}
