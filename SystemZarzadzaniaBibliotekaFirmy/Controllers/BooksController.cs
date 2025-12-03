using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Services;

namespace SystemZarzadzaniaBibliotekaFirmy.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService bookService, ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    private string GetCurrentUsername() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAllBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
            return NotFound();

        return Ok(book);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createBookDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) tworzy nową książkę: {Title}", 
            username, userId, createBookDto.Title);
        
        var book = await _bookService.CreateBookAsync(createBookDto);
        
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) utworzył książkę ID: {BookId}, Tytuł: {Title}", 
            username, userId, book.Id, book.Title);
        
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, UpdateBookDto updateBookDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) aktualizuje książkę ID: {BookId}", 
            username, userId, id);
        
        var book = await _bookService.UpdateBookAsync(id, updateBookDto);
        if (book == null)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował zaktualizować nieistniejącą książkę ID: {BookId}", 
                username, userId, id);
            return NotFound();
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) zaktualizował książkę ID: {BookId}, Tytuł: {Title}", 
            username, userId, book.Id, book.Title);

        return Ok(book);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) usuwa książkę ID: {BookId}", 
            username, userId, id);
        
        var result = await _bookService.DeleteBookAsync(id);
        if (!result)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował usunąć nieistniejącą książkę ID: {BookId}", 
                username, userId, id);
            return NotFound();
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) usunął książkę ID: {BookId}", 
            username, userId, id);

        return NoContent();
    }

    [HttpDelete("bulk")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<object>> DeleteBooksBulk([FromBody] int[] bookIds)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) inicjuje masowe usuwanie {Count} książek", 
            username, userId, bookIds.Length);
        
        if (bookIds == null || bookIds.Length == 0)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował masowo usunąć książki z pustą listą ID", 
                username, userId);
            return BadRequest(new { message = "Lista ID książek nie może być pusta" });
        }

        var deletedCount = await _bookService.DeleteBooksAsync(bookIds);
        
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) masowo usunął {DeletedCount} z {RequestedCount} książek", 
            username, userId, deletedCount, bookIds.Length);

        return Ok(new 
        { 
            message = $"Usunięto {deletedCount} z {bookIds.Length} książek",
            deletedCount = deletedCount,
            requestedCount = bookIds.Length
        });
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks([FromQuery] string term)
    {
        var books = await _bookService.SearchBooksAsync(term);
        return Ok(books);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByCategory(int categoryId)
    {
        var books = await _bookService.GetBooksByCategoryAsync(categoryId);
        return Ok(books);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetAvailableBooks()
    {
        var books = await _bookService.GetAvailableBooksAsync();
        return Ok(books);
    }
}

