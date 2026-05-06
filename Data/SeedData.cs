using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalVehicleService.Models;

namespace RentalVehicleService.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Retry logic for Docker DB startup
                int retries = 5;
                while (retries > 0)
                {
                    try
                    {
                        await context.Database.MigrateAsync();
                        break;
                    }
                    catch (Exception)
                    {
                        retries--;
                        if (retries == 0) throw;
                        Console.WriteLine($"Database not ready, retrying in 5s... ({retries} retries left)");
                        await Task.Delay(5000);
                    }
                }

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
                        new Station { Name = "Trạm Bến Thành", Address = "1 Lê Lợi, Quận 1, TP.HCM", TotalCapacity = 30, CurrentCount = 0, IsActive = true, Latitude = 10.7725, Longitude = 106.6980 },
                        new Station { Name = "Trạm Nhà Rồng", Address = "1 Nguyễn Tất Thành, Quận 4, TP.HCM", TotalCapacity = 25, CurrentCount = 0, IsActive = true, Latitude = 10.7600, Longitude = 106.7050 },
                        new Station { Name = "Trạm Thảo Cầm Viên", Address = "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM", TotalCapacity = 20, CurrentCount = 0, IsActive = true, Latitude = 10.7875, Longitude = 106.7050 },
                        new Station { Name = "Trạm Phú Mỹ Hưng", Address = "801 Nguyễn Văn Linh, Quận 7, TP.HCM", TotalCapacity = 35, CurrentCount = 0, IsActive = true, Latitude = 10.7290, Longitude = 106.7220 },
                        new Station { Name = "Trạm Đại học Bách Khoa", Address = "268 Lý Thường Kiệt, Quận 10, TP.HCM", TotalCapacity = 40, CurrentCount = 0, IsActive = true, Latitude = 10.7730, Longitude = 106.6600 },
                    };

                    var additionalStations = new List<string[]>
                    {
                        new[] { "Trạm Hồ Con Rùa", "Công trường Quốc Tế, Quận 3", "10.7825", "106.6961" },
                        new[] { "Trạm Dinh Độc Lập", "135 Nam Kỳ Khởi Nghĩa, Quận 1", "10.7770", "106.6953" },
                        new[] { "Trạm Nhà Thờ Đức Bà", "1 Công xã Paris, Quận 1", "10.7797", "106.6990" },
                        new[] { "Trạm Bitexco", "2 Hải Triều, Quận 1", "10.7715", "106.7043" },
                        new[] { "Trạm Landmark 81", "720A Điện Biên Phủ, Bình Thạnh", "10.7948", "106.7218" },
                        new[] { "Trạm Công viên Gia Định", "Phường 9, Phú Nhuận", "10.8122", "106.6775" },
                        new[] { "Trạm Chợ Bà Chiểu", "Phường 1, Bình Thạnh", "10.8016", "106.6988" },
                        new[] { "Trạm Chợ Tân Định", "Hai Bà Trưng, Quận 1", "10.7900", "106.6900" },
                        new[] { "Trạm Cầu Ánh Sao", "Quận 7, TP.HCM", "10.7185", "106.7191" },
                        new[] { "Trạm Lotte Mart Quận 7", "Nguyễn Hữu Thọ, Quận 7", "10.7523", "106.7014" },
                        new[] { "Trạm ĐH Tôn Đức Thắng", "Nguyễn Hữu Thọ, Quận 7", "10.7327", "106.6996" },
                        new[] { "Trạm ĐH RMIT", "702 Nguyễn Văn Linh, Quận 7", "10.7297", "106.6942" },
                        new[] { "Trạm SC VivoCity", "Nguyễn Văn Linh, Quận 7", "10.7295", "106.7048" },
                        new[] { "Trạm Crescent Mall", "Tôn Dật Tiên, Quận 7", "10.7288", "106.7188" },
                        new[] { "Trạm Aeon Mall Tân Phú", "Bờ Bao Tân Thắng, Tân Phú", "10.8041", "106.6166" },
                        new[] { "Trạm Aeon Mall Bình Tân", "Đường số 17A, Bình Tân", "10.7436", "106.6081" },
                        new[] { "Trạm Pandora City", "Trường Chinh, Tân Phú", "10.8080", "106.6345" },
                        new[] { "Trạm Coop Mart Quang Trung", "Quang Trung, Gò Vấp", "10.8266", "106.6715" },
                        new[] { "Trạm Mega Market An Phú", "Song Hành, Quận 2", "10.7950", "106.7450" },
                        new[] { "Trạm Parkson Cantavil", "Xa lộ Hà Nội, Quận 2", "10.8000", "106.7420" },
                        new[] { "Trạm Vincom Thảo Điền", "Xa lộ Hà Nội, Quận 2", "10.8015", "106.7525" },
                        new[] { "Trạm Estella Place", "Xa lộ Hà Nội, Quận 2", "10.7985", "106.7485" },
                        new[] { "Trạm Gigamall Thủ Đức", "Phạm Văn Đồng, Thủ Đức", "10.8275", "106.7215" },
                        new[] { "Trạm Vincom Thủ Đức", "Võ Văn Ngân, Thủ Đức", "10.8505", "106.7585" },
                        new[] { "Trạm ĐH Sư Phạm Kỹ Thuật", "Võ Văn Ngân, Thủ Đức", "10.8510", "106.7720" }
                    };

                    var rand = new Random();
                    foreach (var sInfo in additionalStations)
                    {
                        stations.Add(new Station {
                            Name = sInfo[0],
                            Address = sInfo[1] + ", TP.HCM",
                            TotalCapacity = rand.Next(30, 60),
                            CurrentCount = 0,
                            IsActive = true,
                            Latitude = double.Parse(sInfo[2]),
                            Longitude = double.Parse(sInfo[3])
                        });
                    }

                    context.Stations.AddRange(stations);
                    await context.SaveChangesAsync();

                    // Seed Vehicles
                    var stationIds = context.Stations.Select(s => s.StationId).ToList();
                    var vehicles = new List<Vehicle>();
                    var models = new[] { "SaigonBike S1", "SaigonBike S2", "SaigonE Pro", "SaigonE Lite" };

                    // Add 200+ random vehicles
                    for (int i = 0; i < 210; i++)
                    {
                        var model = models[rand.Next(models.Length)];
                        var isElectric = model.Contains("E");
                        var state = (VehicleState)rand.Next(0, 4); // Random Available, Rented, Maintenance, Charging

                        vehicles.Add(new Vehicle {
                            VehicleModel = model,
                            Price = isElectric ? 1500 : 500,
                            BatteryPercentage = rand.Next(20, 100),
                            State = state,
                            Type = isElectric ? VehicleType.Electric : VehicleType.Standard,
                            LastMaintenance = DateTime.Now.AddDays(-rand.Next(1, 45)),
                            CurrentStationId = state == VehicleState.Rented ? null : stationIds[rand.Next(stationIds.Count)]
                        });
                    }

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
        }
    }
}
