using CycleApp.Models;
using CycleApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace CycleApp.Tests.UnitTests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly Mock<ILogger<TokenService>> _loggerMock;
        private readonly IConfiguration _configuration;

        public TokenServiceTests()
        {
            _loggerMock = new Mock<ILogger<TokenService>>();
            
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Key", "0478095303d262bd745ffdd63410478095303d262bd745ffdd0478095303d262bd745ffdd63411bb0b63411bb0b1bb0b-67bd41c2-0478095303d262bd745ffdd63411bb0b5265e34b"},
                {"Jwt:Issuer", "test-issuer"},
                {"Jwt:Audience", "test-audience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _tokenService = new TokenService(_configuration, _loggerMock.Object);
        }

        [Fact]
        public void GenerateToken_ForUser_ShouldIncludeCorrectClaims()
        {
            // Arrange
            var user = new User
            {
                UserId = 123,
                Email = "test@example.com"
            };

            // Act
            var token = _tokenService.GenerateToken(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            Assert.NotNull(emailClaim);
            Assert.Equal(user.Email, emailClaim.Value);
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            Assert.NotNull(userIdClaim);
            Assert.Equal(user.UserId.ToString(), userIdClaim.Value);
        }

        [Fact]
        public void GenerateToken_ForEmail_ShouldIncludeEmailButNoUserId()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var token = _tokenService.GenerateToken(email);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            Assert.NotNull(emailClaim);
            Assert.Equal(email, emailClaim.Value);
            
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            Assert.Null(userIdClaim); // В этом случае ID пользователя должен отсутствовать
        }

        [Fact]
        public void ValidateToken_ShouldReturnTrue_ForValidToken()
        {
            // Arrange
            var user = new User
            {
                UserId = 123,
                Email = "test@example.com"
            };
            var token = _tokenService.GenerateToken(user);

            // Act
            var isValid = _tokenService.ValidateToken(token);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateToken_ShouldReturnFalse_ForInvalidToken()
        {
            // Arrange
            var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

            // Act
            var isValid = _tokenService.ValidateToken(invalidToken);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void GetUserIdFromToken_ShouldReturnCorrectUserId()
        {
            // Arrange
            var user = new User
            {
                UserId = 123,
                Email = "test@example.com"
            };
            var token = _tokenService.GenerateToken(user);

            // Act
            var userId = _tokenService.GetUserIdFromToken(token);

            // Assert
            Assert.Equal(user.UserId, userId);
        }

        [Fact]
        public void GetUserIdFromToken_ShouldReturnZero_ForEmailOnlyToken()
        {
            // Arrange
            var email = "test@example.com";
            var token = _tokenService.GenerateToken(email);

            // Act
            var userId = _tokenService.GetUserIdFromToken(token);

            // Assert
            Assert.Equal(0, userId);
        }

        [Fact]
        public void GetUserIdFromToken_ShouldReturnZero_ForInvalidToken()
        {
            // Arrange
            var invalidToken = "invalid_token";

            // Act
            var userId = _tokenService.GetUserIdFromToken(invalidToken);

            // Assert
            Assert.Equal(0, userId);
        }
    }
}
