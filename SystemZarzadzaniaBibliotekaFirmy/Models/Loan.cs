namespace SystemZarzadzaniaBibliotekaFirmy.Models;

public class Loan
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsReturned { get; set; } = false;
    public string? Notes { get; set; }
    
    // Relacje
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}

