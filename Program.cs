using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Data;
using RentalVehicleService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/Views/Admin/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Admin/Shared/{0}.cshtml");
    });

var app = builder.Build();

// SEED DATA
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Seed Roles
        string[] roles = { "Admin", "LocalUser", "Tourist" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Admin User
        var adminEmail = "admin@saigonride.vn";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                UserType = "System",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed Stations
        if (!context.Stations.Any())
        {
            var stations = new List<Station>
            {
                new Station { Name = "Trạm Bến Thành", Address = "1 Lê Lợi, Quận 1, TP.HCM", TotalCapacity = 30, CurrentCount = 12, IsActive = true, Latitude = 10.7725, Longitude = 106.6980 },
                new Station { Name = "Trạm Nhà Rồng", Address = "1 Nguyễn Tất Thành, Quận 4, TP.HCM", TotalCapacity = 25, CurrentCount = 22, IsActive = true, Latitude = 10.7600, Longitude = 106.7050 },
                new Station { Name = "Trạm Thảo Cầm Viên", Address = "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM", TotalCapacity = 20, CurrentCount = 3, IsActive = true, Latitude = 10.7875, Longitude = 106.7050 },
                new Station { Name = "Trạm Phú Mỹ Hưng", Address = "801 Nguyễn Văn Linh, Quận 7, TP.HCM", TotalCapacity = 35, CurrentCount = 28, IsActive = true, Latitude = 10.7290, Longitude = 106.7220 },
                new Station { Name = "Trạm Đại học Bách Khoa", Address = "268 Lý Thường Kiệt, Quận 10, TP.HCM", TotalCapacity = 40, CurrentCount = 5, IsActive = true, Latitude = 10.7730, Longitude = 106.6600 },
            };
            context.Stations.AddRange(stations);
            await context.SaveChangesAsync();

            // Seed Vehicles
            var stationIds = context.Stations.Select(s => s.StationId).ToList();
            var vehicles = new List<Vehicle>
            {
                new Vehicle { VehicleModel = "SaigonBike S1", Price = 500, BatteryPercentage = 100, State = VehicleState.Available, Type = VehicleType.Standard, LastMaintenance = DateTime.Now.AddDays(-5), CurrentStationId = stationIds[0] },
                new Vehicle { VehicleModel = "SaigonBike S1", Price = 500, BatteryPercentage = 85, State = VehicleState.Available, Type = VehicleType.Standard, LastMaintenance = DateTime.Now.AddDays(-10), CurrentStationId = stationIds[0] },
                new Vehicle { VehicleModel = "SaigonE Pro", Price = 1500, BatteryPercentage = 72, State = VehicleState.Available, Type = VehicleType.Electric, LastMaintenance = DateTime.Now.AddDays(-3), CurrentStationId = stationIds[1] },
                new Vehicle { VehicleModel = "SaigonE Pro", Price = 1500, BatteryPercentage = 45, State = VehicleState.Charging, Type = VehicleType.Electric, LastMaintenance = DateTime.Now.AddDays(-7), CurrentStationId = stationIds[1] },
                new Vehicle { VehicleModel = "SaigonBike S2", Price = 500, BatteryPercentage = 100, State = VehicleState.Available, Type = VehicleType.Standard, LastMaintenance = DateTime.Now.AddDays(-1), CurrentStationId = stationIds[2] },
                new Vehicle { VehicleModel = "SaigonE Lite", Price = 1200, BatteryPercentage = 90, State = VehicleState.Available, Type = VehicleType.Electric, LastMaintenance = DateTime.Now.AddDays(-2), CurrentStationId = stationIds[2] },
                new Vehicle { VehicleModel = "SaigonBike S1", Price = 500, BatteryPercentage = 60, State = VehicleState.Rented, Type = VehicleType.Standard, LastMaintenance = DateTime.Now.AddDays(-15), CurrentStationId = null },
                new Vehicle { VehicleModel = "SaigonE Pro", Price = 1500, BatteryPercentage = 30, State = VehicleState.Rented, Type = VehicleType.Electric, LastMaintenance = DateTime.Now.AddDays(-4), CurrentStationId = null },
                new Vehicle { VehicleModel = "SaigonBike S2", Price = 500, BatteryPercentage = 15, State = VehicleState.Maintenance, Type = VehicleType.Standard, LastMaintenance = DateTime.Now, CurrentStationId = stationIds[3] },
                new Vehicle { VehicleModel = "SaigonE Lite", Price = 1200, BatteryPercentage = 95, State = VehicleState.Available, Type = VehicleType.Electric, LastMaintenance = DateTime.Now.AddDays(-6), CurrentStationId = stationIds[4] },
            };
            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();

            // Seed some completed Rentals
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin != null)
            {
                var rentals = new List<Rental>();
                var random = new Random(42);
                for (int i = 0; i < 7; i++)
                {
                    var day = DateTime.Today.AddDays(-6 + i);
                    var count = random.Next(2, 6);
                    for (int j = 0; j < count; j++)
                    {
                        var vType = j % 2 == 0 ? VehicleType.Standard : VehicleType.Electric;
                        var fare = vType == VehicleType.Standard ? random.Next(10, 50) * 1000m : random.Next(20, 80) * 1000m;
                        rentals.Add(new Rental
                        {
                            UserId = admin.Id,
                            VehicleId = stationIds.Count > 0 ? vehicles[random.Next(0, vehicles.Count)].VehicleId : 1,
                            StartStationId = stationIds[random.Next(stationIds.Count)],
                            EndStationId = stationIds[random.Next(stationIds.Count)],
                            StartTime = day.AddHours(random.Next(6, 20)),
                            EndTime = day.AddHours(random.Next(6, 20)).AddMinutes(random.Next(15, 90)),
                            Status = RentalStatus.Completed,
                            FinalFare = fare,
                            VehicleType = vType
                        });
                    }
                }
                // Add 2 active rentals
                rentals.Add(new Rental
                {
                    UserId = admin.Id,
                    VehicleId = vehicles[6].VehicleId,
                    StartStationId = stationIds[0],
                    StartTime = DateTime.Now.AddMinutes(-25),
                    Status = RentalStatus.Active,
                    FinalFare = 0,
                    VehicleType = VehicleType.Standard
                });
                rentals.Add(new Rental
                {
                    UserId = admin.Id,
                    VehicleId = vehicles[7].VehicleId,
                    StartStationId = stationIds[3],
                    StartTime = DateTime.Now.AddMinutes(-10),
                    Status = RentalStatus.Active,
                    FinalFare = 0,
                    VehicleType = VehicleType.Electric
                });
                context.Rentals.AddRange(rentals);
                await context.SaveChangesAsync();
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
