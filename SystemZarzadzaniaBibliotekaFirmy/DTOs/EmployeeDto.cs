using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaBibliotekaFirmy.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeDto
{
    [Required(ErrorMessage = "Imię jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Imię musi mieć od 2 do 50 znaków")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Nazwisko musi mieć od 2 do 50 znaków")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
    [StringLength(100, ErrorMessage = "Email nie może przekraczać 100 znaków")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dział jest wymagany")]
    [StringLength(100, ErrorMessage = "Nazwa działu nie może przekraczać 100 znaków")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stanowisko jest wymagane")]
    [StringLength(100, ErrorMessage = "Stanowisko nie może przekraczać 100 znaków")]
    public string Position { get; set; } = string.Empty;
}

public class UpdateEmployeeDto
{
    [Required(ErrorMessage = "Imię jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Imię musi mieć od 2 do 50 znaków")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Nazwisko musi mieć od 2 do 50 znaków")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
    [StringLength(100, ErrorMessage = "Email nie może przekraczać 100 znaków")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Dział jest wymagany")]
    [StringLength(100, ErrorMessage = "Nazwa działu nie może przekraczać 100 znaków")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Stanowisko jest wymagane")]
    [StringLength(100, ErrorMessage = "Stanowisko nie może przekraczać 100 znaków")]
    public string Position { get; set; } = string.Empty;
}

