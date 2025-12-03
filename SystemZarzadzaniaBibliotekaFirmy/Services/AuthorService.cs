using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<AuthorService> _logger;

    public AuthorService(LibraryDbContext context, ILogger<AuthorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync()
    {
        return await _context.Authors
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                FullName = a.FullName,
                Biography = a.Biography
            })
            .ToListAsync();
    }

    public async Task<AuthorDto?> GetAuthorByIdAsync(int id)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null) return null;

        return new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            FullName = author.FullName,
            Biography = author.Biography
        };
    }

    public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto)
    {
        _logger.LogInformation("Tworzenie nowego autora: {FullName}", 
            $"{createAuthorDto.FirstName} {createAuthorDto.LastName}");
        
        var author = new Author
        {
            FirstName = createAuthorDto.FirstName,
            LastName = createAuthorDto.LastName,
            Biography = createAuthorDto.Biography
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Autor utworzony pomyślnie. ID: {AuthorId}, Imię i nazwisko: {FullName}", 
            author.Id, author.FullName);

        return new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            FullName = author.FullName,
            Biography = author.Biography
        };
    }

    public async Task<AuthorDto?> UpdateAuthorAsync(int id, UpdateAuthorDto updateAuthorDto)
    {
        _logger.LogInformation("Aktualizacja autora ID: {AuthorId}", id);
        
        var author = await _context.Authors.FindAsync(id);
        if (author == null)
        {
            _logger.LogWarning("Próba aktualizacji nieistniejącego autora ID: {AuthorId}", id);
            return null;
        }

        _logger.LogInformation("Aktualizacja autora: {OldName} -> {NewName}", 
            author.FullName, $"{updateAuthorDto.FirstName} {updateAuthorDto.LastName}");

        author.FirstName = updateAuthorDto.FirstName;
        author.LastName = updateAuthorDto.LastName;
        author.Biography = updateAuthorDto.Biography;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Autor ID: {AuthorId} zaktualizowany pomyślnie", id);

        return new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            FullName = author.FullName,
            Biography = author.Biography
        };
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        _logger.LogInformation("Usuwanie autora ID: {AuthorId}", id);
        
        var author = await _context.Authors.FindAsync(id);
        if (author == null)
        {
            _logger.LogWarning("Próba usunięcia nieistniejącego autora ID: {AuthorId}", id);
            return false;
        }

        _logger.LogInformation("Usuwanie autora: {FullName} (ID: {AuthorId})", author.FullName, author.Id);
        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Autor ID: {AuthorId} usunięty pomyślnie", id);
        
        return true;
    }
}

