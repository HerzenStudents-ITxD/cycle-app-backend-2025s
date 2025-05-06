using CycleApp.Contracts.Auth;
using CycleApp.DataAccess;
using CycleApp.Models;
using CycleApp.Services;
using CycleApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CycleApp.Tests.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ICodeStorageService> _codeStorageMock;
        private readonly DbContextOptions<CycleDbContext> _options;
        
        public AuthServiceTests()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _emailServiceMock = new Mock<IEmailService>();
            _codeStorageMock = new Mock<ICodeStorageService>();
            
            // Настраиваем in-memory базу данных для тестов
            _options = new DbContextOptionsBuilder<CycleDbContext>()
                .UseInMemoryDatabase(databaseName: $"AuthServiceTestDb_{Guid.NewGuid()}")
                .Options;
        }
        
        [Fact]
        public async Task ValidateUserAsync_ShouldReturnTrue_WhenCodeIsValid()
        {
            // Arrange
            var email = "test@example.com";
            var code = "123456";
            
            _codeStorageMock.Setup(x => x.ValidateCode(email, code)).Returns(true);
            
            using var context = new CycleDbContext(_options);
            var authService = new AuthService(_tokenServiceMock.Object, _emailServiceMock.Object, _codeStorageMock.Object, context);
            
            // Act
            var result = await authService.ValidateUserAsync(email, code);
            
            // Assert
            Assert.True(result);
            _codeStorageMock.Verify(x => x.ValidateCode(email, code), Times.Once);
        }
        
        [Fact]
        public async Task ValidateUserAsync_ShouldReturnFalse_WhenCodeIsInvalid()
        {
            // Arrange
            var email = "test@example.com";
            var code = "123456";
            
            _codeStorageMock.Setup(x => x.ValidateCode(email, code)).Returns(false);
            
            using var context = new CycleDbContext(_options);
            var authService = new AuthService(_tokenServiceMock.Object, _emailServiceMock.Object, _codeStorageMock.Object, context);
            
            // Act
            var result = await authService.ValidateUserAsync(email, code);
            
            // Assert
            Assert.False(result);
            _codeStorageMock.Verify(x => x.ValidateCode(email, code), Times.Once);
        }
        
        [Fact]
        public async Task RegisterUserAsync_ShouldCreateNewUser_WhenEmailDoesntExist()
        {
            // Arrange
            var email = "newuser@example.com";
            var request = new RegisterRequest
            {
                Email = email,
                CycleLength = 28,
                PeriodLength = 5
            };
            
            var expectedToken = "test-token";
            _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(expectedToken);
            
            using var context = new CycleDbContext(_options);
            var authService = new AuthService(_tokenServiceMock.Object, _emailServiceMock.Object, _codeStorageMock.Object, context);
            
            // Act
            var result = await authService.RegisterUserAsync(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedToken, result.Token);
            Assert.Equal(email, result.Email);
            Assert.False(result.IsNewUser);
            
            // Проверяем, что пользователь был добавлен в базу
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            Assert.NotNull(user);
            Assert.Equal(28, user.CycleLength);
            Assert.Equal(5, user.PeriodLength);
            Assert.True(user.RemindPeriod);
            Assert.True(user.RemindOvulation);
        }
        
        [Fact]
        public async Task RegisterUserAsync_ShouldReturnNull_WhenEmailAlreadyExists()
        {
            // Arrange
            var email = "existing@example.com";
            var existingUser = new User
            {
                Email = email,
                CycleLength = 28,
                PeriodLength = 5
            };
            
            using var context = new CycleDbContext(_options);
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();
            
            var request = new RegisterRequest
            {
                Email = email,
                CycleLength = 30,
                PeriodLength = 6
            };
            
            var authService = new AuthService(_tokenServiceMock.Object, _emailServiceMock.Object, _codeStorageMock.Object, context);
            
            // Act
            var result = await authService.RegisterUserAsync(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Token); // Токен должен быть null для существующего пользователя
            Assert.Equal(email, result.Email);
            Assert.False(result.IsNewUser);
        }
        
        [Fact]
        public async Task LoginUserAsync_ShouldReturnUserToken_WhenUserExists()
        {
            // Arrange
            var email = "existing@example.com";
            var existingUser = new User
            {
                Email = email,
                CycleLength = 28,
                PeriodLength = 5
            };
            
            using var context = new CycleDbContext(_options);
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();
            
            var expectedToken = "user-token";
            _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(expectedToken);
            
            var authService = new AuthService(_tokenServiceMock.Object, _emailServiceMock.Object, _codeStorageMock.Object, context);
            
            // Act
            var result = await authService.LoginUserAsync(email);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedToken, result.Token);
            Assert.Equal(email, result.Email);
            Assert.False(result.IsNewUser);
        }
        
        [Fact]
        public async Task LoginUserAsync_ShouldReturnRegistrationToken_WhenUserDoesntExist()
        {
            // Arrange
            var email = "newuser@example.com";
            
            var expectedToken = "registration-token";
            _tokenServiceMock.Setup(x => x.GenerateToken(email)).Returns(expectedToken);
            
            using var context = new CycleDbContext(_options);
            var authService = new AuthService(_tokenServiceMock.Object, _emailServiceMock.Object, _codeStorageMock.Object, context);
            
            // Act
            var result = await authService.LoginUserAsync(email);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedToken, result.Token);
            Assert.Equal(email, result.Email);
            Assert.True(result.IsNewUser);
        }
    }
}
