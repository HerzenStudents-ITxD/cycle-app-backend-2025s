using CycleApp.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace CycleApp.Tests.UnitTests.Services
{
    public class CodeStorageServiceTests
    {
        private readonly Mock<ILogger<CodeStorageService>> _loggerMock;
        private readonly CodeStorageService _codeStorageService;

        public CodeStorageServiceTests()
        {
            _loggerMock = new Mock<ILogger<CodeStorageService>>();
            _codeStorageService = new CodeStorageService(_loggerMock.Object);
        }

        [Fact]
        public void StoreCode_ShouldStoreValidCode()
        {
            // Arrange
            var email = "test@example.com";
            var code = "123456";
            var expiration = TimeSpan.FromMinutes(15);

            // Act
            _codeStorageService.StoreCode(email, code, expiration);

            // Assert
            var isValid = _codeStorageService.ValidateCode(email, code);
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateCode_ShouldReturnFalse_WhenCodeIsInvalid()
        {
            // Arrange
            var email = "test@example.com";
            var code = "123456";
            var wrongCode = "654321";
            var expiration = TimeSpan.FromMinutes(15);

            // Act
            _codeStorageService.StoreCode(email, code, expiration);

            // Assert
            var isValid = _codeStorageService.ValidateCode(email, wrongCode);
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateCode_ShouldReturnFalse_WhenCodeIsExpired()
        {
            // Arrange
            var email = "test@example.com";
            var code = "123456";
            var expiration = TimeSpan.FromMilliseconds(1); // Очень короткое время жизни

            // Act
            _codeStorageService.StoreCode(email, code, expiration);
            // Ждем, чтобы код истек
            System.Threading.Thread.Sleep(10);

            // Assert
            var isValid = _codeStorageService.ValidateCode(email, code);
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateCode_ShouldReturnFalse_AfterCodeIsUsed()
        {
            // Arrange
            var email = "test@example.com";
            var code = "123456";
            var expiration = TimeSpan.FromMinutes(15);

            // Act
            _codeStorageService.StoreCode(email, code, expiration);
            
            // Первая проверка должна вернуть true и пометить код как использованный
            var firstValidation = _codeStorageService.ValidateCode(email, code);
            
            // Вторая проверка должна вернуть false, так как код уже использован
            var secondValidation = _codeStorageService.ValidateCode(email, code);

            // Assert
            Assert.True(firstValidation);
            Assert.False(secondValidation);
        }

        [Fact]
        public void InvalidateCode_ShouldMakeCodeInvalid()
        {
            // Arrange
            var email = "test@example.com";
            var code = "123456";
            var expiration = TimeSpan.FromMinutes(15);

            // Act
            _codeStorageService.StoreCode(email, code, expiration);
            _codeStorageService.InvalidateCode(email);

            // Assert
            var isValid = _codeStorageService.ValidateCode(email, code);
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateCode_ShouldBeCaseInsensitiveForEmail()
        {
            // Arrange
            var email = "Test@Example.com";
            var lowerEmail = "test@example.com";
            var code = "123456";
            var expiration = TimeSpan.FromMinutes(15);

            // Act
            _codeStorageService.StoreCode(email, code, expiration);

            // Assert
            var isValid = _codeStorageService.ValidateCode(lowerEmail, code);
            Assert.True(isValid);
        }
    }
}
