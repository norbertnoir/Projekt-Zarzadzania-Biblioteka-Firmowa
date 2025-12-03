using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SystemZarzadzaniaBibliotekaFirmy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly LibraryDbContext _context;

        public ReportsController(LibraryDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        [AllowAnonymous]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalEmployees = await _context.Employees.CountAsync();
            
            var activeLoans = await _context.Loans
                .Where(l => l.ReturnDate == null)
                .CountAsync();

            var overdueLoans = await _context.Loans
                .Where(l => l.ReturnDate == null && l.DueDate < DateTime.UtcNow)
                .CountAsync();

            return Ok(new DashboardStatsDto
            {
                TotalBooks = totalBooks,
                TotalEmployees = totalEmployees,
                ActiveLoans = activeLoans,
                OverdueLoans = overdueLoans
            });
        }

        [HttpGet("export/books")]
        public async Task<IActionResult> ExportBooks()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Id,Tytuł,ISBN,Wydawca,Rok,Kategoria,Autorzy,Dostępna");

            foreach (var book in books)
            {
                var authors = string.Join(";", book.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}"));
                var line = $"{book.Id},\"{book.Title}\",{book.ISBN},\"{book.Publisher}\",{book.Year},\"{book.Category?.Name}\",\"{authors}\",{book.IsAvailable}";
                csv.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"ksiazki_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet("export/loans")]
        public async Task<IActionResult> ExportLoans()
        {
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Employee)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Id,Książka,Pracownik,Data Wypożyczenia,Termin,Data Zwrotu,Status");

            foreach (var loan in loans)
            {
                var status = loan.ReturnDate.HasValue ? "Zwrócona" : (loan.DueDate < DateTime.UtcNow ? "Przeterminowana" : "Wypożyczona");
                var returnDate = loan.ReturnDate.HasValue ? loan.ReturnDate.Value.ToString("yyyy-MM-dd") : "";
                
                var line = $"{loan.Id},\"{loan.Book?.Title}\",\"{loan.Employee?.FirstName} {loan.Employee?.LastName}\",{loan.LoanDate:yyyy-MM-dd},{loan.DueDate:yyyy-MM-dd},{returnDate},{status}";
                csv.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"wypozyczenia_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        [HttpGet("export/books/pdf")]
        public async Task<IActionResult> ExportBooksPdf()
        {
            // Pobranie danych z bazy wraz z relacjami (Kategorie, Autorzy)
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .ToListAsync();

            // Tworzenie dokumentu PDF przy użyciu QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Ustawienia strony (format A4, marginesy)
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Nagłówek raportu
                    page.Header()
                        .Text("Raport Książek")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    // Treść raportu - tabela
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            // Definicja kolumn (szerokości relatywne)
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Tytuł
                                columns.RelativeColumn(2); // Autorzy
                                columns.RelativeColumn(2); // Kategoria
                                columns.RelativeColumn(1); // Rok
                                columns.RelativeColumn(1); // Status
                            });

                            // Nagłówek tabeli
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Tytuł");
                                header.Cell().Element(CellStyle).Text("Autorzy");
                                header.Cell().Element(CellStyle).Text("Kategoria");
                                header.Cell().Element(CellStyle).Text("Rok");
                                header.Cell().Element(CellStyle).Text("Status");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var book in books)
                            {
                                var authors = string.Join(", ", book.BookAuthors.Select(ba => $"{ba.Author.FirstName} {ba.Author.LastName}"));
                                
                                table.Cell().Element(CellStyle).Text(book.Title);
                                table.Cell().Element(CellStyle).Text(authors);
                                table.Cell().Element(CellStyle).Text(book.Category?.Name ?? "-");
                                table.Cell().Element(CellStyle).Text(book.Year.ToString());
                                table.Cell().Element(CellStyle).Text(book.IsAvailable ? "Dostępna" : "Wypożyczona");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Strona ");
                            x.CurrentPageNumber();
                        });
                });
            });

            var bytes = document.GeneratePdf();
            return File(bytes, "application/pdf", $"ksiazki_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

        [HttpGet("export/loans/pdf")]
        public async Task<IActionResult> ExportLoansPdf()
        {
            var loans = await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Employee)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("Raport Wypożyczeń")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Książka
                                columns.RelativeColumn(2); // Pracownik
                                columns.RelativeColumn(2); // Data Wyp.
                                columns.RelativeColumn(2); // Termin
                                columns.RelativeColumn(2); // Status
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Książka");
                                header.Cell().Element(CellStyle).Text("Pracownik");
                                header.Cell().Element(CellStyle).Text("Data Wyp.");
                                header.Cell().Element(CellStyle).Text("Termin");
                                header.Cell().Element(CellStyle).Text("Status");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            foreach (var loan in loans)
                            {
                                var status = loan.ReturnDate.HasValue ? "Zwrócona" : (loan.DueDate < DateTime.UtcNow ? "Przeterminowana" : "Wypożyczona");
                                
                                table.Cell().Element(CellStyle).Text(loan.Book?.Title ?? "-");
                                table.Cell().Element(CellStyle).Text($"{loan.Employee?.FirstName} {loan.Employee?.LastName}");
                                table.Cell().Element(CellStyle).Text(loan.LoanDate.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).Text(loan.DueDate.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).Text(status);

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Strona ");
                            x.CurrentPageNumber();
                        });
                });
            });

            var bytes = document.GeneratePdf();
            return File(bytes, "application/pdf", $"wypozyczenia_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }
}
