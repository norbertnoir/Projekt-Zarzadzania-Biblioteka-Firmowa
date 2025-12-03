using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaBibliotekaFirmy.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Pages { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<AuthorDto> Authors { get; set; } = new();
}

public class CreateBookDto
{
    [Required(ErrorMessage = "Tytuł jest wymagany")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Tytuł musi mieć od 1 do 200 znaków")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "ISBN jest wymagany")]
    [RegularExpression(@"^(?:\d{13}|\d{10})$", ErrorMessage = "ISBN musi mieć 10 lub 13 cyfr")]
    public string ISBN { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wydawca jest wymagany")]
    [StringLength(100, ErrorMessage = "Nazwa wydawcy nie może przekraczać 100 znaków")]
    public string Publisher { get; set; } = string.Empty;

    [Range(1000, 2100, ErrorMessage = "Rok musi być między 1000 a 2100")]
    public int Year { get; set; }

    [Range(1, 10000, ErrorMessage = "Liczba stron musi być między 1 a 10000")]
    public int Pages { get; set; }

    [StringLength(2000, ErrorMessage = "Opis nie może przekraczać 2000 znaków")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategoria jest wymagana")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Musisz wybrać przynajmniej jednego autora")]
    [MinLength(1, ErrorMessage = "Musisz wybrać przynajmniej jednego autora")]
    public List<int> AuthorIds { get; set; } = new();
}

public class UpdateBookDto
{
    [Required(ErrorMessage = "Tytuł jest wymagany")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Tytuł musi mieć od 1 do 200 znaków")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "ISBN jest wymagany")]
    [RegularExpression(@"^(?:\d{13}|\d{10})$", ErrorMessage = "ISBN musi mieć 10 lub 13 cyfr")]
    public string ISBN { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wydawca jest wymagany")]
    [StringLength(100, ErrorMessage = "Nazwa wydawcy nie może przekraczać 100 znaków")]
    public string Publisher { get; set; } = string.Empty;

    [Range(1000, 2100, ErrorMessage = "Rok musi być między 1000 a 2100")]
    public int Year { get; set; }

    [Range(1, 10000, ErrorMessage = "Liczba stron musi być między 1 a 10000")]
    public int Pages { get; set; }

    [StringLength(2000, ErrorMessage = "Opis nie może przekraczać 2000 znaków")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategoria jest wymagana")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Musisz wybrać przynajmniej jednego autora")]
    [MinLength(1, ErrorMessage = "Musisz wybrać przynajmniej jednego autora")]
    public List<int> AuthorIds { get; set; } = new();
}

