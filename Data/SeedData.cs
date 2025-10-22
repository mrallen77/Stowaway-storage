using Microsoft.EntityFrameworkCore;
using StowawayStorage.Models;

namespace StowawayStorage.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        if (await db.StorageUnits.AnyAsync()) return;

        var units = Enumerable.Range(1, 20).Select(i => new StorageUnit
        {
            Name = "Small Locker A",
            Size = "5x5",
            MonthlyPrice = 39.99m,
            IsActive = true
        });

        await db.StorageUnits.AddRangeAsync(units);
        await db.SaveChangesAsync();
    }
}
