using Xunit;
using Moq;
using RentalVehicleService.Services;
using RentalVehicleService.Models;
using RentalVehicleService.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaiGonRide.Tests
{
    public class RentalServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void CalculateFare_StandardVehicle_30Min_Returns15000()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            var startTime = DateTime.Now;
            var endTime = startTime.AddMinutes(30);
            decimal pricePerMin = 500m;

            // Act
            var result = service.CalculateFare(startTime, endTime, pricePerMin);

            // Assert
            Assert.Equal(15000m, result);
        }

        [Fact]
        public void CalculateFare_ElectricVehicle_60Min_Returns90000()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            var startTime = DateTime.Now;
            var endTime = startTime.AddMinutes(60);
            decimal pricePerMin = 1500m;

            // Act
            var result = service.CalculateFare(startTime, endTime, pricePerMin);

            // Assert
            Assert.Equal(90000m, result);
        }

        [Fact]
        public void CalculateFare_FractionalDuration_RoundsUp()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            var startTime = DateTime.Now;
            var endTime = startTime.AddMinutes(1.2); // 1 min 12 sec -> should round to 2 min
            decimal pricePerMin = 500m;

            // Act
            var result = service.CalculateFare(startTime, endTime, pricePerMin);

            // Assert
            Assert.Equal(1000m, result); // 2 * 500
        }

        [Fact]
        public void CalculateFare_NegativeDuration_ReturnsZero()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            var startTime = DateTime.Now;
            var endTime = startTime.AddMinutes(-10);
            decimal pricePerMin = 500m;

            // Act
            var result = service.CalculateFare(startTime, endTime, pricePerMin);

            // Assert
            Assert.Equal(0m, result);
        }

        [Theory]
        [InlineData(0, 0)]      // 0 min -> 0 VND
        [InlineData(1, 500)]    // 1 min -> 500 VND
        [InlineData(0.1, 500)]  // 0.1 min -> rounds to 1 min -> 500 VND
        [InlineData(0.9, 500)]  // 0.9 min -> rounds to 1 min -> 500 VND
        [InlineData(1.1, 1000)] // 1.1 min -> rounds to 2 min -> 1000 VND
        public void CalculateFare_BoundaryValues_ReturnsCorrectFare(double minutes, decimal expectedFare)
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            var startTime = DateTime.Now;
            var endTime = startTime.AddMinutes(minutes);
            decimal pricePerMin = 500m;

            // Act
            var result = service.CalculateFare(startTime, endTime, pricePerMin);

            // Assert
            Assert.Equal(expectedFare, result);
        }

        [Fact]
        public void CheckDiscount_LowInventoryStation_Returns15Percent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var station = new Station 
            { 
                StationId = 1, 
                Name = "Low Station", 
                TotalCapacity = 10, 
                CurrentCount = 1, // 10% < 20%
                IsActive = true 
            };
            context.Stations.Add(station);
            context.SaveChanges();

            var service = new RentalService(context);

            // Act
            var result = service.CheckDiscount(1);

            // Assert
            Assert.Equal(0.15m, result);
        }

        [Fact]
        public void CheckDiscount_NormalInventoryStation_ReturnsZero()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var station = new Station 
            { 
                StationId = 2, 
                Name = "Normal Station", 
                TotalCapacity = 10, 
                CurrentCount = 5, // 50% > 20%
                IsActive = true 
            };
            context.Stations.Add(station);
            context.SaveChanges();

            var service = new RentalService(context);

            // Act
            var result = service.CheckDiscount(2);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void ProcessFinalBill_WithDiscount_CalculatesCorrectly()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var station = new Station 
            { 
                StationId = 10, 
                TotalCapacity = 10, 
                CurrentCount = 1, 
                IsActive = true 
            };
            var vehicle = new Vehicle 
            { 
                VehicleModel = "Test Model",
                Price = 1000, // 1000 VND/min
                State = VehicleState.Available 
            };
            context.Stations.Add(station);
            context.Vehicles.Add(vehicle);
            context.SaveChanges();

            var startTime = DateTime.Now.AddMinutes(-10);
            var rental = new Rental 
            { 
                RentalId = 1, 
                VehicleId = vehicle.VehicleId, 
                StartTime = startTime 
            };

            var endTime = startTime.AddMinutes(10);
            var service = new RentalService(context);

            // Act
            // Base fare = 10 * 1000 = 10000
            // Discount = 10000 * 0.15 = 1500
            // Final = 8500
            var finalFare = service.ProcessFinalBill(rental, 10, endTime);

            // Assert
            Assert.Equal(8500m, finalFare);
            Assert.Equal(1500m, rental.DiscountAmount);
        }
    }
}
