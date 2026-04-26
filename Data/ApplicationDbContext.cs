using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Models;
using RentalVehicleService.Models.ViewModels;

namespace RentalVehicleService.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Station> Stations { get; set; }
        public DbSet<RentalVehicleService.Models.Vehicle> Vehicles { get; set; }
        public DbSet<RentalVehicleService.Models.Rental> Rentals { get; set; }
        public DbSet<RentalVehicleService.Models.BugReport> BugReport { get; set; } = default!;
    }
}
