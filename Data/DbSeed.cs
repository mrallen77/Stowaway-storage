using Microsoft.EntityFrameworkCore;
using StowawayStorage.Models;

namespace StowawayStorage.Data
{
    public static class DbSeed
    {
        public static async Task EnsureUnitsAsync(ApplicationDbContext db)
        {
            await db.Database.MigrateAsync();

            if (!await db.StorageUnits.AnyAsync())
            {
                db.StorageUnits.AddRange(
                    new StorageUnit { Name = "Small Locker A", Size = "5x5", MonthlyPrice = 39.99m, IsActive = true },
                    new StorageUnit { Name = "Standard Unit B", Size = "5x10", MonthlyPrice = 79.99m, IsActive = true },
                    new StorageUnit { Name = "Large Unit C", Size = "10x10", MonthlyPrice = 129.99m, IsActive = true }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
