using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaBibliotekaFirmy.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Nazwa użytkownika musi mieć od 3 do 50 znaków")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć od 6 do 100 znaków")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Nazwa użytkownika musi mieć od 3 do 50 znaków")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
    [StringLength(100, ErrorMessage = "Email nie może przekraczać 100 znaków")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć od 6 do 100 znaków")]
    public string Password { get; set; } = string.Empty;

    [RegularExpression(@"^(Employee|Librarian|Admin)$", ErrorMessage = "Rola musi być: Employee, Librarian lub Admin")]
    public string? Role { get; set; } = "Employee";

    [Range(1, int.MaxValue, ErrorMessage = "ID pracownika musi być większe od 0")]
    public int? EmployeeId { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
}

