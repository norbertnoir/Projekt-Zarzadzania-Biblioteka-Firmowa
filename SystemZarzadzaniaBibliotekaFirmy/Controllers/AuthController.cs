using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Services;

namespace SystemZarzadzaniaBibliotekaFirmy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        // Próba logowania użytkownika przez serwis autoryzacji
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
        {
            return Unauthorized(new { message = "Nieprawidłowa nazwa użytkownika lub hasło" });
        }

        // Zwrócenie tokenu JWT i danych użytkownika w przypadku sukcesu
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        // Sprawdź czy istnieją już użytkownicy - jeśli nie, pozwól na rejestrację pierwszego admina
        var hasUsers = await _authService.HasAnyUsersAsync();
        if (hasUsers)
        {
            // Jeśli są już użytkownicy, wymagaj autoryzacji (tylko Admin lub Bibliotekarz może dodawać nowych)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { message = "Wymagana autoryzacja do rejestracji nowych użytkowników" });
            }

            var isAuthorized = User.IsInRole("Admin") || User.IsInRole("Librarian");
            if (!isAuthorized)
            {
                return Forbid();
            }
        }
        else
        {
            // Jeśli nie ma użytkowników, pierwszy użytkownik musi być Adminem (bootstrap systemu)
            registerDto.Role = "Admin";
        }

        try
        {
            // Rejestracja użytkownika i wygenerowanie tokenu
            var result = await _authService.RegisterAsync(registerDto);
            return CreatedAtAction(nameof(GetUser), new { id = result.Username }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
        if (!result)
        {
            return BadRequest(new { message = "Nieprawidłowe obecne hasło" });
        }

        return Ok(new { message = "Hasło zostało zmienione" });
    }

    [HttpGet("debug/users")]
    public async Task<IActionResult> DebugUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(new 
        { 
            count = users.Count(),
            users = users.Select(u => new 
            { 
                id = u.Id,
                username = u.Username,
                email = u.Email,
                role = u.Role,
                isActive = u.IsActive,
                employeeId = u.EmployeeId
            })
        });
    }
}

