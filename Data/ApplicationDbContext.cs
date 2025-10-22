using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StowawayStorage.Models;

namespace StowawayStorage.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<StorageUnit> StorageUnits => Set<StorageUnit>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StorageUnit>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(120).IsRequired();
                e.Property(x => x.Size).HasMaxLength(60).IsRequired();
                e.Property(x => x.MonthlyPrice).HasColumnType("decimal(10,2)");
            });

            builder.Entity<Reservation>(e =>
            {
                e.HasOne(r => r.Unit)
                 .WithMany(u => u.Reservations)
                 .HasForeignKey(r => r.UnitId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.Property(r => r.CreatedUtc).HasDefaultValueSql("GETUTCDATE()");

                // Helpful index for overlap lookups
                e.HasIndex(r => new { r.UnitId, r.StartDateUtc, r.EndDateUtc });
            });
        }
    }
}
