using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaBibliotekaFirmy.DTOs;

public class AuthorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Biography { get; set; }
}

public class CreateAuthorDto
{
    [Required(ErrorMessage = "Imię autora jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Imię musi mieć od 2 do 50 znaków")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko autora jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Nazwisko musi mieć od 2 do 50 znaków")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Biografia nie może przekraczać 1000 znaków")]
    public string? Biography { get; set; }
}

public class UpdateAuthorDto
{
    [Required(ErrorMessage = "Imię autora jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Imię musi mieć od 2 do 50 znaków")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko autora jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Nazwisko musi mieć od 2 do 50 znaków")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Biografia nie może przekraczać 1000 znaków")]
    public string? Biography { get; set; }
}

