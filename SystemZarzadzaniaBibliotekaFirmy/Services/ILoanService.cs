using SystemZarzadzaniaBibliotekaFirmy.DTOs;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public interface ILoanService
{
    Task<IEnumerable<LoanDto>> GetAllLoansAsync();
    Task<LoanDto?> GetLoanByIdAsync(int id);
    Task<LoanDto> CreateLoanAsync(CreateLoanDto createLoanDto);
    Task<LoanDto?> ReturnLoanAsync(int id, ReturnLoanDto returnLoanDto);
    Task<IEnumerable<LoanDto>> GetLoansByEmployeeAsync(int employeeId);
    Task<IEnumerable<LoanDto>> GetActiveLoansAsync();
    Task<IEnumerable<LoanDto>> GetOverdueLoansAsync();
}

