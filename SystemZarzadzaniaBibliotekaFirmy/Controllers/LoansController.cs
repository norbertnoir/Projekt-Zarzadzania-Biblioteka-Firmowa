using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Services;

namespace SystemZarzadzaniaBibliotekaFirmy.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ILogger<LoansController> _logger;

    public LoansController(ILoanService loanService, ILogger<LoansController> logger)
    {
        _loanService = loanService;
        _logger = logger;
    }

    private string GetCurrentUsername() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetAllLoans()
    {
        var loans = await _loanService.GetAllLoansAsync();
        return Ok(loans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LoanDto>> GetLoan(int id)
    {
        var loan = await _loanService.GetLoanByIdAsync(id);
        if (loan == null)
            return NotFound();

        return Ok(loan);
    }

    [HttpPost]
    public async Task<ActionResult<LoanDto>> CreateLoan(CreateLoanDto createLoanDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        
        // Logika zabezpieczająca przed wypożyczaniem dla innych osób (chyba że jest się adminem)
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var isAdmin = userRole == "Admin" || userRole == "Administrator" || userRole == "Librarian";

        if (!isAdmin)
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim) || !int.TryParse(employeeIdClaim, out int employeeId))
            {
                _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował wypożyczyć książkę, ale nie ma powiązanego pracownika.", username, userId);
                return BadRequest("Twój użytkownik nie jest powiązany z żadnym pracownikiem. Skontaktuj się z administratorem.");
            }

            // Wymuś użycie ID pracownika powiązanego z zalogowanym użytkownikiem
            createLoanDto.EmployeeId = employeeId;
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) inicjuje wypożyczenie - Książka ID: {BookId}, Pracownik ID: {EmployeeId}", 
            username, userId, createLoanDto.BookId, createLoanDto.EmployeeId);
        
        try
        {
            var loan = await _loanService.CreateLoanAsync(createLoanDto);
            
            _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) pomyślnie utworzył wypożyczenie ID: {LoanId}, Książka: {BookTitle}, Pracownik: {EmployeeId}", 
                username, userId, loan.Id, loan.BookTitle, loan.EmployeeId);
            
            return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) - błąd podczas tworzenia wypożyczenia: {Error}", 
                username, userId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) - błąd podczas tworzenia wypożyczenia: {Error}", 
                username, userId, ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/return")]
    public async Task<ActionResult<LoanDto>> ReturnLoan(int id, ReturnLoanDto returnLoanDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) inicjuje zwrot wypożyczenia ID: {LoanId}", 
            username, userId, id);
        
        try
        {
            var loan = await _loanService.ReturnLoanAsync(id, returnLoanDto);
            if (loan == null)
            {
                _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował zwrócić nieistniejące wypożyczenie ID: {LoanId}", 
                    username, userId, id);
                return NotFound();
            }

            _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) pomyślnie zwrócił wypożyczenie ID: {LoanId}, Książka: {BookTitle}", 
                username, userId, loan.Id, loan.BookTitle);

            return Ok(loan);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) - błąd podczas zwrotu wypożyczenia ID: {LoanId}: {Error}", 
                username, userId, id, ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoansByEmployee(int employeeId)
    {
        var loans = await _loanService.GetLoansByEmployeeAsync(employeeId);
        return Ok(loans);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetActiveLoans()
    {
        var loans = await _loanService.GetActiveLoansAsync();
        return Ok(loans);
    }

    [HttpGet("overdue")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<IEnumerable<LoanDto>>> GetOverdueLoans()
    {
        var loans = await _loanService.GetOverdueLoansAsync();
        return Ok(loans);
    }
}

