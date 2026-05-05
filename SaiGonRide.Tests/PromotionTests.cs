using Xunit;
using RentalVehicleService.Services;
using RentalVehicleService.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace SaiGonRide.Tests
{
    public class PromotionTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void ApplyCoupon_ValidCode_Deducts10000()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            decimal currentFare = 25000m;
            string coupon = "SAIGONGREEN20";

            // Act
            var result = service.ApplyCoupon(currentFare, coupon);

            // Assert
            Assert.Equal(15000m, result);
        }

        [Fact]
        public void ApplyCoupon_ValidCode_FloorsAtZero()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            decimal currentFare = 5000m; // Fare less than 10k
            string coupon = "SAIGONGREEN20";

            // Act
            var result = service.ApplyCoupon(currentFare, coupon);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void ApplyCoupon_InvalidCode_NoChange()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            decimal currentFare = 25000m;
            string coupon = "INVALID_CODE";

            // Act
            var result = service.ApplyCoupon(currentFare, coupon);

            // Assert
            Assert.Equal(25000m, result);
        }

        [Fact]
        public void ApplyCoupon_EmptyCode_NoChange()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            decimal currentFare = 25000m;

            // Act
            var result = service.ApplyCoupon(currentFare, "");

            // Assert
            Assert.Equal(25000m, result);
        }
    }
}
