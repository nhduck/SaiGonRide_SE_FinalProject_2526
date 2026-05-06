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

        [Theory]
        [InlineData(10000, 0)]      // Boundary: Exactly 10k -> 0
        [InlineData(10001, 1)]      // Boundary: 10,001 -> 1
        [InlineData(9999, 0)]       // Boundary: 9,999 -> 0
        [InlineData(20000, 10000)]  // Normal Case
        public void ApplyCoupon_BoundaryAmounts_ReturnsExpected(decimal currentFare, decimal expectedFare)
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            string coupon = "SAIGONGREEN20";

            // Act
            var result = service.ApplyCoupon(currentFare, coupon);

            // Assert
            Assert.Equal(expectedFare, result);
        }

        [Fact]
        public void ApplyCoupon_CaseInsensitive_WorksForLowercase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new RentalService(context);
            decimal currentFare = 20000m;
            string coupon = "saigongreen20"; // lowercase

            // Act
            var result = service.ApplyCoupon(currentFare, coupon);

            // Assert
            Assert.Equal(10000m, result);
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
