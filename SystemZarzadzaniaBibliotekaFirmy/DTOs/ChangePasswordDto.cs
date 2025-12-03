using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaBibliotekaFirmy.DTOs;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Obecne hasło jest wymagane")]
    public string OldPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nowe hasło jest wymagane")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć od 6 do 100 znaków")]
    public string NewPassword { get; set; } = string.Empty;
}

