using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;
using SystemZarzadzaniaBibliotekaFirmy.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace SystemZarzadzaniaBibliotekaFirmy.Tests.Services;

public class LoanServiceTests
{
    private LibraryDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new LibraryDbContext(options);
        
        // Seed test data
        context.Categories.Add(new Category { Id = 1, Name = "Test Category", Description = "Test" });
        context.Books.Add(new Book 
        { 
            Id = 1, 
            Title = "Test Book", 
            ISBN = "1234567890", 
            Publisher = "Test", 
            Year = 2023, 
            Pages = 300,
            CategoryId = 1,
            IsAvailable = true
        });
        context.Employees.Add(new Employee 
        { 
            Id = 1, 
            FirstName = "Test", 
            LastName = "Employee", 
            Email = "test@test.com",
            Department = "IT",
            Position = "Developer"
        });
        context.SaveChanges();

        return context;
    }

    private ILogger<LoanService> GetLogger()
    {
        return new Mock<ILogger<LoanService>>().Object;
    }

    [Fact]
    public async Task CreateLoanAsync_WithValidData_CreatesLoan()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var service = new LoanService(context, logger);
        var createDto = new CreateLoanDto
        {
            BookId = 1,
            EmployeeId = 1,
            DueDate = DateTime.UtcNow.AddDays(14),
            Notes = "Test loan"
        };

        // Act
        var result = await service.CreateLoanAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.BookId);
        Assert.Equal(1, result.EmployeeId);
        Assert.False(result.IsReturned);
    }

    [Fact]
    public async Task CreateLoanAsync_WithUnavailableBook_ThrowsException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var book = await context.Books.FindAsync(1);
        if (book != null)
        {
            book.IsAvailable = false;
            await context.SaveChangesAsync();
        }

        var logger = GetLogger();
        var service = new LoanService(context, logger);
        var createDto = new CreateLoanDto
        {
            BookId = 1,
            EmployeeId = 1,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateLoanAsync(createDto));
    }

    [Fact]
    public async Task ReturnLoanAsync_WithValidLoan_ReturnsLoan()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var service = new LoanService(context, logger);
        
        var createDto = new CreateLoanDto
        {
            BookId = 1,
            EmployeeId = 1,
            DueDate = DateTime.UtcNow.AddDays(14)
        };
        var loan = await service.CreateLoanAsync(createDto);

        var returnDto = new ReturnLoanDto
        {
            ReturnDate = DateTime.UtcNow,
            Notes = "Returned"
        };

        // Act
        var result = await service.ReturnLoanAsync(loan.Id, returnDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsReturned);
        Assert.NotNull(result.ReturnDate);
    }

    [Fact]
    public async Task GetOverdueLoansAsync_ReturnsOverdueLoans()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var logger = GetLogger();
        var service = new LoanService(context, logger);
        
        var createDto = new CreateLoanDto
        {
            BookId = 1,
            EmployeeId = 1,
            DueDate = DateTime.UtcNow.AddDays(-1) // Overdue
        };
        await service.CreateLoanAsync(createDto);

        // Act
        var result = await service.GetOverdueLoansAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}

