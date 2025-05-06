using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CycleApp.Tests.UnitTests.Services
{
    public class CycleAnalyticsServiceTests
    {
        private readonly DbContextOptions<CycleDbContext> _options;
        private readonly Mock<ILogger<CycleAnalyticsService>> _loggerMock;
        
        public CycleAnalyticsServiceTests()
        {
            _loggerMock = new Mock<ILogger<CycleAnalyticsService>>();
            
            // Настраиваем in-memory базу данных для тестов
            _options = new DbContextOptionsBuilder<CycleDbContext>()
                .UseInMemoryDatabase(databaseName: $"CycleAnalyticsTestDb_{Guid.NewGuid()}")
                .Options;
        }
        
        [Fact]
        public async Task GetCycleAnalytics_ShouldReturnNull_WhenUserDoesntExist()
        {
            // Arrange
            using var context = new CycleDbContext(_options);
            var service = new CycleAnalyticsService(context, _loggerMock.Object);
            
            // Act
            var result = await service.GetCycleAnalytics(999); // Несуществующий ID пользователя
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task GetCycleAnalytics_ShouldReturnAnalytics_ForUserWithPeriods()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                UserId = userId,
                Email = "test@example.com",
                CycleLength = 28,
                PeriodLength = 5
            };
            
            // Создаем историю менструаций
            var now = DateTime.UtcNow;
            var periods = new[]
            {
                new Period { UserId = userId, StartDate = now.AddDays(-28*3), EndDate = now.AddDays(-28*3+5), IsActive = false },
                new Period { UserId = userId, StartDate = now.AddDays(-28*2), EndDate = now.AddDays(-28*2+5), IsActive = false },
                new Period { UserId = userId, StartDate = now.AddDays(-28), EndDate = now.AddDays(-28+5), IsActive = false }
            };
            
            using var context = new CycleDbContext(_options);
            context.Users.Add(user);
            context.Periods.AddRange(periods);
            await context.SaveChangesAsync();
            
            var service = new CycleAnalyticsService(context, _loggerMock.Object);
            
            // Act
            var result = await service.GetCycleAnalytics(userId) as dynamic;
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.PreviousPeriods);
            Assert.Equal(3, result.PreviousPeriods.Count);
        }
        
        [Fact]
        public async Task GetAverageCycleDuration_ShouldCalculateCorrectly()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                UserId = userId,
                Email = "test@example.com",
                CycleLength = 28,
                PeriodLength = 5
            };
            
            // Создаем историю менструаций с известными циклами (28 и 30 дней)
            var now = DateTime.UtcNow;
            var periods = new[]
            {
                new Period { UserId = userId, StartDate = now.AddDays(-58), EndDate = now.AddDays(-53), IsActive = false }, // -58
                new Period { UserId = userId, StartDate = now.AddDays(-28), EndDate = now.AddDays(-23), IsActive = false }, // -28 (цикл 30 дней)
                new Period { UserId = userId, StartDate = now, EndDate = now.AddDays(5), IsActive = false } // 0 (цикл 28 дней)
            };
            
            using var context = new CycleDbContext(_options);
            context.Users.Add(user);
            context.Periods.AddRange(periods);
            await context.SaveChangesAsync();
            
            var service = new CycleAnalyticsService(context, _loggerMock.Object);
            
            // Act
            var result = await service.GetAverageCycleDuration(userId) as dynamic;
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.CyclesAnalyzed);
            // Средняя продолжительность должна быть близка к 29 дням (28+30)/2
            // Из-за точности часов, может быть отклонение в 1 день
            Assert.InRange(result.AverageDays, 28, 30);
        }
        
        [Fact]
        public async Task GetAveragePeriodDuration_ShouldCalculateCorrectly()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                UserId = userId,
                Email = "test@example.com",
                CycleLength = 28,
                PeriodLength = 5
            };
            
            // Создаем историю менструаций с разной продолжительностью (5 и 6 дней)
            var now = DateTime.UtcNow;
            var periods = new[]
            {
                new Period { UserId = userId, StartDate = now.AddDays(-56), EndDate = now.AddDays(-51), IsActive = false }, // 5 дней
                new Period { UserId = userId, StartDate = now.AddDays(-28), EndDate = now.AddDays(-22), IsActive = false }, // 6 дней
                new Period { UserId = userId, StartDate = now, EndDate = now.AddDays(5), IsActive = false } // 5 дней
            };
            
            using var context = new CycleDbContext(_options);
            context.Users.Add(user);
            context.Periods.AddRange(periods);
            await context.SaveChangesAsync();
            
            var service = new CycleAnalyticsService(context, _loggerMock.Object);
            
            // Act
            var result = await service.GetAveragePeriodDuration(userId) as dynamic;
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.PeriodsAnalyzed);
            // Средняя продолжительность должна быть близка к 5.33 дням (5+6+5)/3
            Assert.InRange(result.AverageDays, 5.3, 5.4);
            Assert.Equal(5, result.MinDays);
            Assert.Equal(6, result.MaxDays);
        }
        
        [Fact]
        public async Task GetRegularityAnalysis_ShouldClassifyRegularityCorrently()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                UserId = userId,
                Email = "test@example.com",
                CycleLength = 28,
                PeriodLength = 5
            };
            
            // Создаем историю менструаций с небольшой вариацией (27, 28, 29 дней)
            var now = DateTime.UtcNow;
            var periods = new[]
            {
                new Period { UserId = userId, StartDate = now.AddDays(-84), EndDate = now.AddDays(-79), IsActive = false },
                new Period { UserId = userId, StartDate = now.AddDays(-56), EndDate = now.AddDays(-51), IsActive = false }, // Цикл 28 дней
                new Period { UserId = userId, StartDate = now.AddDays(-29), EndDate = now.AddDays(-24), IsActive = false }, // Цикл 27 дней
                new Period { UserId = userId, StartDate = now, EndDate = now.AddDays(5), IsActive = false } // Цикл 29 дней
            };
            
            using var context = new CycleDbContext(_options);
            context.Users.Add(user);
            context.Periods.AddRange(periods);
            await context.SaveChangesAsync();
            
            var service = new CycleAnalyticsService(context, _loggerMock.Object);
            
            // Act
            var result = await service.GetRegularityAnalysis(userId) as dynamic;
            
            // Assert
            Assert.NotNull(result);
            // Вариация 2 дня (29-27), должна быть классифицирована как "Очень регулярный"
            Assert.Equal("Очень регулярный", result.Regularity);
        }
    }
}
