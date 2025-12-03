using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public class LoanService : ILoanService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LibraryDbContext context, ILogger<LoanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<LoanDto>> GetAllLoansAsync()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Employee)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                LoanDate = l.LoanDate,
                ReturnDate = l.ReturnDate,
                DueDate = l.DueDate,
                IsReturned = l.IsReturned,
                Notes = l.Notes,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FullName
            })
            .ToListAsync();
    }

    public async Task<LoanDto?> GetLoanByIdAsync(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null) return null;

        return new LoanDto
        {
            Id = loan.Id,
            LoanDate = loan.LoanDate,
            ReturnDate = loan.ReturnDate,
            DueDate = loan.DueDate,
            IsReturned = loan.IsReturned,
            Notes = loan.Notes,
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            EmployeeId = loan.EmployeeId,
            EmployeeName = loan.Employee.FullName
        };
    }

    public async Task<LoanDto> CreateLoanAsync(CreateLoanDto createLoanDto)
    {
        _logger.LogInformation("Rozpoczęcie procesu wypożyczenia - Książka ID: {BookId}, Pracownik ID: {EmployeeId}", 
            createLoanDto.BookId, createLoanDto.EmployeeId);

        // Krok 1: Weryfikacja - sprawdzenie istnienia książki
        var book = await _context.Books.FindAsync(createLoanDto.BookId);
        if (book == null)
        {
            _logger.LogWarning("Weryfikacja nieudana: Książka ID {BookId} nie została znaleziona", createLoanDto.BookId);
            throw new ArgumentException("Książka nie została znaleziona");
        }

        // Krok 2: Weryfikacja - sprawdzenie dostępności książki
        if (!book.IsAvailable)
        {
            _logger.LogWarning("Weryfikacja nieudana: Książka ID {BookId} ({Title}) nie jest dostępna", 
                book.Id, book.Title);
            throw new InvalidOperationException("Książka nie jest dostępna");
        }

        // Krok 3: Weryfikacja - sprawdzenie istnienia pracownika
        var employee = await _context.Employees.FindAsync(createLoanDto.EmployeeId);
        if (employee == null)
        {
            _logger.LogWarning("Weryfikacja nieudana: Pracownik ID {EmployeeId} nie został znaleziony", createLoanDto.EmployeeId);
            throw new ArgumentException("Pracownik nie został znaleziony");
        }

        _logger.LogInformation("Weryfikacja zakończona pomyślnie - Książka: {BookTitle}, Pracownik: {EmployeeName}", 
            book.Title, employee.FullName);

        // Krok 4: Zatwierdzenie - utworzenie wypożyczenia
        var loan = new Loan
        {
            BookId = createLoanDto.BookId,
            EmployeeId = createLoanDto.EmployeeId,
            LoanDate = DateTime.UtcNow,
            DueDate = createLoanDto.DueDate,
            Notes = createLoanDto.Notes,
            IsReturned = false
        };

        // Oznacz książkę jako niedostępną
        book.IsAvailable = false;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Wypożyczenie zatwierdzone i utworzone pomyślnie - Loan ID: {LoanId}, Książka: {BookTitle}, Pracownik: {EmployeeName}, Termin zwrotu: {DueDate}", 
            loan.Id, book.Title, employee.FullName, loan.DueDate);

        return (await GetLoanByIdAsync(loan.Id))!;
    }

    public async Task<LoanDto?> ReturnLoanAsync(int id, ReturnLoanDto returnLoanDto)
    {
        _logger.LogInformation("Rozpoczęcie procesu zwrotu wypożyczenia ID: {LoanId}", id);
        
        var loan = await _context.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            _logger.LogWarning("Próba zwrotu nieistniejącego wypożyczenia ID: {LoanId}", id);
            return null;
        }

        if (loan.IsReturned)
        {
            _logger.LogWarning("Próba zwrotu już zwróconej książki - Loan ID: {LoanId}, Książka: {BookTitle}", 
                id, loan.Book.Title);
            throw new InvalidOperationException("Książka została już zwrócona");
        }

        _logger.LogInformation("Zwrot książki: {BookTitle}, Pracownik: {EmployeeId}, Data zwrotu: {ReturnDate}", 
            loan.Book.Title, loan.EmployeeId, returnLoanDto.ReturnDate);

        loan.ReturnDate = returnLoanDto.ReturnDate;
        loan.IsReturned = true;
        loan.Notes = returnLoanDto.Notes ?? loan.Notes;

        // Oznacz książkę jako dostępną
        loan.Book.IsAvailable = true;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Zwrot zatwierdzony pomyślnie - Loan ID: {LoanId}, Książka: {BookTitle} oznaczona jako dostępna", 
            id, loan.Book.Title);

        return await GetLoanByIdAsync(loan.Id);
    }

    public async Task<IEnumerable<LoanDto>> GetLoansByEmployeeAsync(int employeeId)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Employee)
            .Where(l => l.EmployeeId == employeeId)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                LoanDate = l.LoanDate,
                ReturnDate = l.ReturnDate,
                DueDate = l.DueDate,
                IsReturned = l.IsReturned,
                Notes = l.Notes,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FullName
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanDto>> GetActiveLoansAsync()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Employee)
            .Where(l => !l.IsReturned)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                LoanDate = l.LoanDate,
                ReturnDate = l.ReturnDate,
                DueDate = l.DueDate,
                IsReturned = l.IsReturned,
                Notes = l.Notes,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FullName
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanDto>> GetOverdueLoansAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Employee)
            .Where(l => !l.IsReturned && l.DueDate < now)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                LoanDate = l.LoanDate,
                ReturnDate = l.ReturnDate,
                DueDate = l.DueDate,
                IsReturned = l.IsReturned,
                Notes = l.Notes,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee.FullName
            })
            .ToListAsync();
    }
}

