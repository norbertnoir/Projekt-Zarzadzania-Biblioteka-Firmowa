using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;
using Microsoft.Extensions.Logging;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public class AuthService : IAuthService
{
    private readonly LibraryDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(LibraryDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        _logger.LogInformation("Próba logowania dla użytkownika: {Username}", loginDto.Username);
        
        // Sprawdź wszystkich użytkowników dla debugowania
        var allUsers = await _context.Users.Select(u => u.Username).ToListAsync();
        _logger.LogInformation("Użytkownicy w bazie: {Users}", string.Join(", ", allUsers));
        
        // Pobierz użytkownika z bazy danych wraz z powiązanym pracownikiem
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == loginDto.Username.ToLower() && u.IsActive);

        if (user == null)
        {
            _logger.LogWarning("Użytkownik nie znaleziony lub nieaktywny: {Username}", loginDto.Username);
            return null;
        }

        _logger.LogInformation("Znaleziono użytkownika: {Username}, ID: {UserId}, IsActive: {IsActive}", 
            user.Username, user.Id, user.IsActive);

        // Weryfikacja hasła (porównanie hasha)
        var passwordValid = VerifyPassword(loginDto.Password, user.PasswordHash);
        _logger.LogInformation("Weryfikacja hasła dla użytkownika {Username}: {IsValid}", 
            user.Username, passwordValid);

        if (!passwordValid)
        {
            _logger.LogWarning("Nieprawidłowe hasło dla użytkownika: {Username}", loginDto.Username);
            return null;
        }

        // Aktualizuj czas ostatniego logowania
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generowanie tokenu JWT
        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24); // Token ważny 24 godziny

        // Zwrócenie odpowiedzi z tokenem
        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Sprawdź czy użytkownik już istnieje
        if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
        {
            throw new InvalidOperationException("Użytkownik o podanej nazwie już istnieje");
        }

        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            throw new InvalidOperationException("Użytkownik o podanym emailu już istnieje");
        }

        // Sprawdź czy EmployeeId istnieje (jeśli podano)
        if (registerDto.EmployeeId.HasValue)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == registerDto.EmployeeId.Value);
            if (!employeeExists)
            {
                throw new ArgumentException("Pracownik o podanym ID nie istnieje");
            }
        }

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = HashPassword(registerDto.Password),
            Role = registerDto.Role ?? "Employee",
            IsActive = true,
            EmployeeId = registerDto.EmployeeId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            EmployeeId = user.EmployeeId,
            EmployeeName = user.Employee?.FullName
        };
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Employee)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                EmployeeId = u.EmployeeId,
                EmployeeName = u.Employee != null ? u.Employee.FullName : null
            })
            .ToListAsync();
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !VerifyPassword(oldPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasAnyUsersAsync()
    {
        return await _context.Users.AnyAsync();
    }

    private string GenerateJwtToken(User user)
    {
        // Pobranie konfiguracji JWT z appsettings.json
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey nie jest skonfigurowany");
        var issuer = jwtSettings["Issuer"] ?? "LibraryManagement";
        var audience = jwtSettings["Audience"] ?? "LibraryManagementUsers";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440"); // 24 godziny

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Definicja "roszczeń" (claims) - danych zaszyfrowanych w tokenie
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("EmployeeId", user.EmployeeId?.ToString() ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Utworzenie tokenu
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            if (string.IsNullOrEmpty(passwordHash))
            {
                _logger.LogWarning("PasswordHash jest pusty");
                return false;
            }

            var result = BCrypt.Net.BCrypt.Verify(password, passwordHash);
            if (!result)
            {
                _logger.LogWarning("Weryfikacja hasła nie powiodła się. PasswordHash length: {Length}", passwordHash.Length);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas weryfikacji hasła");
            return false;
        }
    }
}

