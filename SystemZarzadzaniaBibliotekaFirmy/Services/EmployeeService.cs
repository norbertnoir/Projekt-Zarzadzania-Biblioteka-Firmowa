using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public class EmployeeService : IEmployeeService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(LibraryDbContext context, ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
    {
        return await _context.Employees
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                FullName = e.FullName,
                Email = e.Email,
                Department = e.Department,
                Position = e.Position,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return null;

        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Email = employee.Email,
            Department = employee.Department,
            Position = employee.Position,
            CreatedAt = employee.CreatedAt
        };
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
    {
        _logger.LogInformation("Tworzenie nowego pracownika: {FullName}, Email: {Email}, Dział: {Department}", 
            $"{createEmployeeDto.FirstName} {createEmployeeDto.LastName}", createEmployeeDto.Email, createEmployeeDto.Department);
        
        var employee = new Employee
        {
            FirstName = createEmployeeDto.FirstName,
            LastName = createEmployeeDto.LastName,
            Email = createEmployeeDto.Email,
            Department = createEmployeeDto.Department,
            Position = createEmployeeDto.Position,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pracownik utworzony pomyślnie. ID: {EmployeeId}, Imię i nazwisko: {FullName}", 
            employee.Id, employee.FullName);

        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Email = employee.Email,
            Department = employee.Department,
            Position = employee.Position,
            CreatedAt = employee.CreatedAt
        };
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateEmployeeDto)
    {
        _logger.LogInformation("Aktualizacja pracownika ID: {EmployeeId}", id);
        
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            _logger.LogWarning("Próba aktualizacji nieistniejącego pracownika ID: {EmployeeId}", id);
            return null;
        }

        _logger.LogInformation("Aktualizacja pracownika: {OldName} -> {NewName}, Email: {Email}", 
            employee.FullName, $"{updateEmployeeDto.FirstName} {updateEmployeeDto.LastName}", updateEmployeeDto.Email);

        employee.FirstName = updateEmployeeDto.FirstName;
        employee.LastName = updateEmployeeDto.LastName;
        employee.Email = updateEmployeeDto.Email;
        employee.Department = updateEmployeeDto.Department;
        employee.Position = updateEmployeeDto.Position;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Pracownik ID: {EmployeeId} zaktualizowany pomyślnie", id);

        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Email = employee.Email,
            Department = employee.Department,
            Position = employee.Position,
            CreatedAt = employee.CreatedAt
        };
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        _logger.LogInformation("Usuwanie pracownika ID: {EmployeeId}", id);
        
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            _logger.LogWarning("Próba usunięcia nieistniejącego pracownika ID: {EmployeeId}", id);
            return false;
        }

        _logger.LogInformation("Usuwanie pracownika: {FullName} (ID: {EmployeeId})", employee.FullName, employee.Id);
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Pracownik ID: {EmployeeId} usunięty pomyślnie", id);
        
        return true;
    }
}

