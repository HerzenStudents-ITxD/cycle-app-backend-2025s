using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using CycleApp.Services;
using CycleApp.Models;
using CycleApp.DataAccess;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;

namespace CycleApp.Tests
{
    public class IntegrationTests : IDisposable
    {
        private readonly CycleDbContext _context;
        private readonly ICycleCalculatorService _calculator;
        private readonly CycleCalculationBackgroundService _backgroundService;
        private readonly Mock<ILogger<CycleCalculationBackgroundService>> _loggerMock;

        public IntegrationTests()
        {
            var options = new DbContextOptionsBuilder<CycleDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=CycleAppTest;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;

            _context = new CycleDbContext(options);
            _context.Database.EnsureDeleted(); // Ensure clean state
            _context.Database.EnsureCreated(); // Create schema based on current model

            _calculator = new CycleCalculatorService();
            _loggerMock = new Mock<ILogger<CycleCalculationBackgroundService>>();
            _backgroundService = new CycleCalculationBackgroundService(
                Mock.Of<IServiceProvider>(),
                _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessUser_CreatesPredictions()
        {
            // Arrange
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                CycleLength = 28,
                PeriodLength = 5,
                Periods = new List<Period>
                {
                    new Period
                    {
                        StartDate = DateTime.UtcNow.AddDays(-28),
                        EndDate = DateTime.UtcNow.AddDays(-23),
                        IsPredicted = false
                    }
                }
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            await _backgroundService.ProcessUserWithRetry(user, _context, _calculator, CancellationToken.None);

            // Assert
            var predictions = await _context.Periods
                .Where(p => p.UserId == user.UserId && p.IsPredicted)
                .ToListAsync();

            Assert.Equal(3, predictions.Count); // Should create 3 future predictions
            Assert.All(predictions, p => Assert.True(p.IsPredicted));
        }

        [Fact]
        public async Task ProcessUser_UpdatesCycleVariations()
        {
            // Arrange
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = "test@example.com",
                CycleLength = 28,
                PeriodLength = 5,
                Periods = new List<Period>
                {
                    new Period
                    {
                        StartDate = DateTime.UtcNow.AddDays(-56),
                        EndDate = DateTime.UtcNow.AddDays(-51),
                        IsPredicted = false
                    },
                    new Period
                    {
                        StartDate = DateTime.UtcNow.AddDays(-28),
                        EndDate = DateTime.UtcNow.AddDays(-23),
                        IsPredicted = false
                    }
                }
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            await _backgroundService.ProcessUserWithRetry(user, _context, _calculator, CancellationToken.None);

            // Assert
            var updatedUser = await _context.Users
                .Include(u => u.Periods)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            Assert.NotNull(updatedUser);
            Assert.Equal(28, updatedUser.CycleLength); // Should maintain average cycle length
            Assert.Equal(5, updatedUser.PeriodLength); // Should maintain period length
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 