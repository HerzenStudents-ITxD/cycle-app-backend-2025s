using System;
using Xunit;
using CycleApp.Services;
using CycleApp.Models;
using System.Collections.Generic;

namespace CycleApp.CycleApp.Tests
{
    public partial class CycleCalculatorTests
    {
        private readonly ICycleCalculatorService _calculator;

        public CycleCalculatorTests()
        {
            _calculator = new CycleCalculatorService();
        }

        [Fact]
        public void CalculateNextPeriod_NoPreviousPeriods_ReturnsExpectedDate()
        {
            // Arrange
            var user = new User
            {
                CycleLength = 28,
                PeriodLength = 5
            };

            // Act
            var (startDate, endDate) = _calculator.CalculateNextPeriod(user, DateTime.UtcNow);

            // Assert
            Assert.Equal(DateTime.UtcNow.Date, startDate.Date);
            Assert.Equal(DateTime.UtcNow.AddDays(5).Date, endDate.Date);
        }

        [Fact]
        public void CalculateNextPeriod_WithPreviousPeriod_ReturnsExpectedDate()
        {
            // Arrange
            var lastPeriod = DateTime.UtcNow.AddDays(-28);
            var user = new User
            {
                CycleLength = 28,
                PeriodLength = 5,
                Periods = new List<Period>
                {
                    new Period
                    {
                        StartDate = lastPeriod,
                        EndDate = lastPeriod.AddDays(5),
                        IsPredicted = false
                    }
                }
            };

            // Act
            var (startDate, endDate) = _calculator.CalculateNextPeriod(user, DateTime.UtcNow);

            // Assert
            Assert.Equal(lastPeriod.AddDays(28).Date, startDate.Date);
            Assert.Equal(lastPeriod.AddDays(33).Date, endDate.Date);
        }

        [Fact]
        public void CalculateNextOvulation_NoPreviousPeriods_ReturnsExpectedDate()
        {
            // Arrange
            var user = new User
            {
                CycleLength = 28,
                PeriodLength = 5
            };

            // Act
            var (startDate, endDate) = _calculator.CalculateNextOvulation(user, DateTime.UtcNow);

            // Assert
            Assert.Equal(DateTime.UtcNow.AddDays(14).Date, startDate.Date);
            Assert.Equal(DateTime.UtcNow.AddDays(16).Date, endDate.Date);
        }

        [Fact]
        public void CalculateNextOvulation_WithPreviousPeriod_ReturnsExpectedDate()
        {
            // Arrange
            var lastPeriod = DateTime.UtcNow.AddDays(-28);
            var user = new User
            {
                CycleLength = 28,
                PeriodLength = 5,
                Periods = new List<Period>
                {
                    new Period
                    {
                        StartDate = lastPeriod,
                        EndDate = lastPeriod.AddDays(5),
                        IsPredicted = false
                    }
                }
            };

            // Act
            var (startDate, endDate) = _calculator.CalculateNextOvulation(user, DateTime.UtcNow);

            // Assert
            Assert.Equal(lastPeriod.AddDays(14).Date, startDate.Date);
            Assert.Equal(lastPeriod.AddDays(16).Date, endDate.Date);
        }

        [Fact]
        public void CalculateDayOfCycle_WithPreviousPeriod_ReturnsCorrectDay()
        {
            // Arrange
            var lastPeriod = DateTime.UtcNow.AddDays(-10);
            var user = new User
            {
                CycleLength = 28,
                PeriodLength = 5,
                Periods = new List<Period>
                {
                    new Period
                    {
                        StartDate = lastPeriod,
                        EndDate = lastPeriod.AddDays(5),
                        IsPredicted = false
                    }
                }
            };

            // Act
            var dayOfCycle = _calculator.CalculateDayOfCycle(user, DateTime.UtcNow);

            // Assert
            Assert.Equal(10, dayOfCycle);
        }

        [Fact]
        public void CalculateDayOfCycle_NoPreviousPeriod_ReturnsOne()
        {
            // Arrange
            var user = new User
            {
                CycleLength = 28,
                PeriodLength = 5
            };

            // Act
            var dayOfCycle = _calculator.CalculateDayOfCycle(user, DateTime.UtcNow);

            // Assert
            Assert.Equal(1, dayOfCycle);
        }
    }
} 