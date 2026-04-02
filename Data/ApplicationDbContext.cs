using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Models;

namespace RentalVehicleService.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Station> Stations { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rental → StartStation (restrict delete)
            builder.Entity<Rental>()
                .HasOne(r => r.StartStation)
                .WithMany()
                .HasForeignKey(r => r.StartStationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rental → EndStation (restrict delete)
            builder.Entity<Rental>()
                .HasOne(r => r.EndStation)
                .WithMany()
                .HasForeignKey(r => r.EndStationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rental → Vehicle (restrict delete)
            builder.Entity<Rental>()
                .HasOne(r => r.Vehicle)
                .WithMany()
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle → Station
            builder.Entity<Vehicle>()
                .HasOne(v => v.CurrentStation)
                .WithMany(s => s.Vehicles)
                .HasForeignKey(v => v.CurrentStationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
