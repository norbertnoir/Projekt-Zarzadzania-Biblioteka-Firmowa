using SystemZarzadzaniaBibliotekaFirmy.DTOs;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto);
    Task<EmployeeDto?> UpdateEmployeeAsync(int id, UpdateEmployeeDto updateEmployeeDto);
    Task<bool> DeleteEmployeeAsync(int id);
}

