using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;
using SystemZarzadzaniaBibliotekaFirmy.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace SystemZarzadzaniaBibliotekaFirmy.Tests.Services;

public class BookServiceTests
{
    private LibraryDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new LibraryDbContext(options);
        
        // Seed test data
        context.Categories.Add(new Category { Id = 1, Name = "Test Category", Description = "Test" });
        context.Authors.Add(new Author { Id = 1, FirstName = "Test", LastName = "Author" });
        context.SaveChanges();

        return context;
    }

    private ILogger<BookService> GetLogger()
    {
        return new Mock<ILogger<BookService>>().Object;
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsAllBooks()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var service = new BookService(context, logger);

        // Act
        var result = await service.GetAllBooksAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<BookDto>>(result);
    }

    [Fact]
    public async Task CreateBookAsync_WithValidData_CreatesBook()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var service = new BookService(context, logger);
        var createDto = new CreateBookDto
        {
            Title = "Test Book",
            ISBN = "1234567890",
            Publisher = "Test Publisher",
            Year = 2023,
            Pages = 300,
            Description = "Test Description",
            CategoryId = 1,
            AuthorIds = new List<int> { 1 }
        };

        // Act
        var result = await service.CreateBookAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Book", result.Title);
        Assert.Equal("1234567890", result.ISBN);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithValidId_ReturnsBook()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var service = new BookService(context, logger);
        
        var createDto = new CreateBookDto
        {
            Title = "Test Book",
            ISBN = "1234567890",
            Publisher = "Test Publisher",
            Year = 2023,
            Pages = 300,
            Description = "Test Description",
            CategoryId = 1,
            AuthorIds = new List<int> { 1 }
        };
        var created = await service.CreateBookAsync(createDto);

        // Act
        var result = await service.GetBookByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test Book", result.Title);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var service = new BookService(context, logger);

        // Act
        var result = await service.GetBookByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchBooksAsync_WithMatchingTerm_ReturnsBooks()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var service = new BookService(context, logger);
        
        var createDto = new CreateBookDto
        {
            Title = "Test Book",
            ISBN = "1234567890",
            Publisher = "Test Publisher",
            Year = 2023,
            Pages = 300,
            Description = "Test Description",
            CategoryId = 1,
            AuthorIds = new List<int> { 1 }
        };
        await service.CreateBookAsync(createDto);

        // Act
        var result = await service.SearchBooksAsync("Test");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}

