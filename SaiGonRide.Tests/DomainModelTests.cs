using Xunit;
using RentalVehicleService.Models;
using System;

namespace SaiGonRide.Tests
{
    public class DomainModelTests
    {
        [Theory]
        [InlineData(100, 10, true)]  // 10% -> true
        [InlineData(100, 19, true)]  // 19% -> true
        [InlineData(100, 20, false)] // 20% -> false
        [InlineData(100, 50, false)] // 50% -> false
        public void Station_IsLowInventory_ReturnsExpected(int capacity, int current, bool expected)
        {
            // Arrange
            var station = new Station 
            { 
                TotalCapacity = capacity, 
                CurrentCount = current 
            };

            // Act
            var result = station.IsLowInventory;

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(100, 86, true)]  // 86% -> true
        [InlineData(100, 90, true)]  // 90% -> true
        [InlineData(100, 85, false)] // 85% -> false
        [InlineData(100, 50, false)] // 50% -> false
        public void Station_IsAlmostFull_ReturnsExpected(int capacity, int current, bool expected)
        {
            // Arrange
            var station = new Station 
            { 
                TotalCapacity = capacity, 
                CurrentCount = current 
            };

            // Act
            var result = station.IsAlmostFull;

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(21, true)]  // 21% -> true
        [InlineData(20, false)] // 20% -> false
        [InlineData(10, false)] // 10% -> false
        [InlineData(100, true)] // 100% -> true
        public void Vehicle_IsReadyForRent_BasedOnBattery(int battery, bool expected)
        {
            // Arrange
            var vehicle = new Vehicle { BatteryPercentage = battery };

            // Act
            var result = vehicle.IsReadyForRent;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Station_FillRate_CalculatesCorrectly()
        {
            // Arrange
            var station = new Station { TotalCapacity = 200, CurrentCount = 50 };

            // Act & Assert
            Assert.Equal(0.25, station.FillRate);
        }
    }
}
