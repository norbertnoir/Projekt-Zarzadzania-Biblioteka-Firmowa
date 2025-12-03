using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;
using SystemZarzadzaniaBibliotekaFirmy.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace SystemZarzadzaniaBibliotekaFirmy.Tests.Services;

public class AuthServiceTests
{
    private LibraryDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    private IConfiguration GetConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"JwtSettings:SecretKey", "YourSuperSecretKeyForJWTTokenGenerationThatShouldBeAtLeast32CharactersLong!"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpirationMinutes", "60"}
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private ILogger<AuthService> GetLogger()
    {
        return new Mock<ILogger<AuthService>>().Object;
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUser()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var configuration = GetConfiguration();
        var logger = GetLogger();
        var service = new AuthService(context, configuration, logger);
        
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@test.com",
            Password = "Test123!",
            Role = "Employee"
        };

        // Act
        var result = await service.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var configuration = GetConfiguration();
        var logger = GetLogger();
        var service = new AuthService(context, configuration, logger);
        
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@test.com",
            Password = "Test123!",
            Role = "Employee"
        };
        await service.RegisterAsync(registerDto);

        var loginDto = new LoginDto
        {
            Username = "testuser",
            Password = "Test123!"
        };

        // Act
        var result = await service.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var configuration = GetConfiguration();
        var logger = GetLogger();
        var service = new AuthService(context, configuration, logger);
        
        var loginDto = new LoginDto
        {
            Username = "invalid",
            Password = "wrongpassword"
        };

        // Act
        var result = await service.LoginAsync(loginDto);

        // Assert
        Assert.Null(result);
    }
}

