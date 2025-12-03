using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaBibliotekaFirmy.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Nazwa kategorii jest wymagana")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nazwa kategorii musi mieć od 2 do 100 znaków")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Opis nie może przekraczać 500 znaków")]
    public string Description { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Nazwa kategorii jest wymagana")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nazwa kategorii musi mieć od 2 do 100 znaków")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Opis nie może przekraczać 500 znaków")]
    public string Description { get; set; } = string.Empty;
}

