using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaBibliotekaFirmy.DTOs;

public class LoanDto
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsReturned { get; set; }
    public string? Notes { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
}

public class CreateLoanDto
{
    [Required(ErrorMessage = "ID książki jest wymagane")]
    [Range(1, int.MaxValue, ErrorMessage = "ID książki musi być większe od 0")]
    public int BookId { get; set; }

    [Required(ErrorMessage = "ID pracownika jest wymagane")]
    [Range(1, int.MaxValue, ErrorMessage = "ID pracownika musi być większe od 0")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Data zwrotu jest wymagana")]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; }

    [StringLength(500, ErrorMessage = "Notatki nie mogą przekraczać 500 znaków")]
    public string? Notes { get; set; }
}

public class ReturnLoanDto
{
    [DataType(DataType.DateTime)]
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    [StringLength(500, ErrorMessage = "Notatki nie mogą przekraczać 500 znaków")]
    public string? Notes { get; set; }
}

