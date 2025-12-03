using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Services;

namespace SystemZarzadzaniaBibliotekaFirmy.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    private string GetCurrentUsername() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployees()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto createEmployeeDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) tworzy nowego pracownika: {FullName}", 
            username, userId, $"{createEmployeeDto.FirstName} {createEmployeeDto.LastName}");
        
        var employee = await _employeeService.CreateEmployeeAsync(createEmployeeDto);
        
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) utworzył pracownika ID: {EmployeeId}, Imię i nazwisko: {FullName}", 
            username, userId, employee.Id, employee.FullName);
        
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeDto>> UpdateEmployee(int id, UpdateEmployeeDto updateEmployeeDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) aktualizuje pracownika ID: {EmployeeId}", 
            username, userId, id);
        
        var employee = await _employeeService.UpdateEmployeeAsync(id, updateEmployeeDto);
        if (employee == null)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował zaktualizować nieistniejącego pracownika ID: {EmployeeId}", 
                username, userId, id);
            return NotFound();
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) zaktualizował pracownika ID: {EmployeeId}, Imię i nazwisko: {FullName}", 
            username, userId, employee.Id, employee.FullName);

        return Ok(employee);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) usuwa pracownika ID: {EmployeeId}", 
            username, userId, id);
        
        var result = await _employeeService.DeleteEmployeeAsync(id);
        if (!result)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował usunąć nieistniejącego pracownika ID: {EmployeeId}", 
                username, userId, id);
            return NotFound();
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) usunął pracownika ID: {EmployeeId}", 
            username, userId, id);

        return NoContent();
    }
}

