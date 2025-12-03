using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Services;

namespace SystemZarzadzaniaBibliotekaFirmy.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;
    private readonly ILogger<AuthorsController> _logger;

    public AuthorsController(IAuthorService authorService, ILogger<AuthorsController> logger)
    {
        _authorService = authorService;
        _logger = logger;
    }

    private string GetCurrentUsername() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAllAuthors()
    {
        var authors = await _authorService.GetAllAuthorsAsync();
        return Ok(authors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(int id)
    {
        var author = await _authorService.GetAuthorByIdAsync(id);
        if (author == null)
            return NotFound();

        return Ok(author);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorDto createAuthorDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) tworzy nowego autora: {FullName}", 
            username, userId, $"{createAuthorDto.FirstName} {createAuthorDto.LastName}");
        
        var author = await _authorService.CreateAuthorAsync(createAuthorDto);
        
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) utworzył autora ID: {AuthorId}, Imię i nazwisko: {FullName}", 
            username, userId, author.Id, author.FullName);
        
        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, author);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<AuthorDto>> UpdateAuthor(int id, UpdateAuthorDto updateAuthorDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) aktualizuje autora ID: {AuthorId}", 
            username, userId, id);
        
        var author = await _authorService.UpdateAuthorAsync(id, updateAuthorDto);
        if (author == null)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował zaktualizować nieistniejącego autora ID: {AuthorId}", 
                username, userId, id);
            return NotFound();
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) zaktualizował autora ID: {AuthorId}, Imię i nazwisko: {FullName}", 
            username, userId, author.Id, author.FullName);

        return Ok(author);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) usuwa autora ID: {AuthorId}", 
            username, userId, id);
        
        var result = await _authorService.DeleteAuthorAsync(id);
        if (!result)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował usunąć nieistniejącego autora ID: {AuthorId}", 
                username, userId, id);
            return NotFound();
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) usunął autora ID: {AuthorId}", 
            username, userId, id);

        return NoContent();
    }
}

