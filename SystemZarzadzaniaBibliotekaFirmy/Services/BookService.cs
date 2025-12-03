using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<BookService> _logger;

    public BookService(LibraryDbContext context, ILogger<BookService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Publisher = b.Publisher,
                Year = b.Year,
                Pages = b.Pages,
                Description = b.Description,
                IsAvailable = b.IsAvailable,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                Authors = b.BookAuthors.Select(ba => new AuthorDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName,
                    FullName = ba.Author.FullName,
                    Biography = ba.Author.Biography
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        var book = await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null) return null;

        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ISBN = book.ISBN,
            Publisher = book.Publisher,
            Year = book.Year,
            Pages = book.Pages,
            Description = book.Description,
            IsAvailable = book.IsAvailable,
            CategoryId = book.CategoryId,
            CategoryName = book.Category.Name,
            Authors = book.BookAuthors.Select(ba => new AuthorDto
            {
                Id = ba.Author.Id,
                FirstName = ba.Author.FirstName,
                LastName = ba.Author.LastName,
                FullName = ba.Author.FullName,
                Biography = ba.Author.Biography
            }).ToList()
        };
    }

    public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto)
    {
        _logger.LogInformation("Tworzenie nowej książki: {Title}, ISBN: {ISBN}", createBookDto.Title, createBookDto.ISBN);
        
        var book = new Book
        {
            Title = createBookDto.Title,
            ISBN = createBookDto.ISBN,
            Publisher = createBookDto.Publisher,
            Year = createBookDto.Year,
            Pages = createBookDto.Pages,
            Description = createBookDto.Description,
            CategoryId = createBookDto.CategoryId,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Książka utworzona pomyślnie. ID: {BookId}, Tytuł: {Title}", book.Id, book.Title);

        // Dodaj autorów
        foreach (var authorId in createBookDto.AuthorIds)
        {
            var bookAuthor = new BookAuthor
            {
                BookId = book.Id,
                AuthorId = authorId
            };
            _context.BookAuthors.Add(bookAuthor);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Autorzy przypisani do książki ID: {BookId}, Liczba autorów: {AuthorCount}", book.Id, createBookDto.AuthorIds.Count);

        return (await GetBookByIdAsync(book.Id))!;
    }

    public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto updateBookDto)
    {
        _logger.LogInformation("Aktualizacja książki ID: {BookId}", id);
        
        var book = await _context.Books
            .Include(b => b.BookAuthors)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            _logger.LogWarning("Próba aktualizacji nieistniejącej książki ID: {BookId}", id);
            return null;
        }

        _logger.LogInformation("Aktualizacja książki: {OldTitle} -> {NewTitle}, ISBN: {ISBN}", book.Title, updateBookDto.Title, updateBookDto.ISBN);

        book.Title = updateBookDto.Title;
        book.ISBN = updateBookDto.ISBN;
        book.Publisher = updateBookDto.Publisher;
        book.Year = updateBookDto.Year;
        book.Pages = updateBookDto.Pages;
        book.Description = updateBookDto.Description;
        book.CategoryId = updateBookDto.CategoryId;
        book.UpdatedAt = DateTime.UtcNow;

        // Aktualizuj autorów
        var existingAuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToList();
        var newAuthorIds = updateBookDto.AuthorIds;

        // Usuń autorów, którzy nie są już w liście
        var authorsToRemove = book.BookAuthors
            .Where(ba => !newAuthorIds.Contains(ba.AuthorId))
            .ToList();
        _context.BookAuthors.RemoveRange(authorsToRemove);

        // Dodaj nowych autorów
        var authorsToAdd = newAuthorIds
            .Where(aid => !existingAuthorIds.Contains(aid))
            .Select(aid => new BookAuthor { BookId = book.Id, AuthorId = aid });
        _context.BookAuthors.AddRange(authorsToAdd);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Książka ID: {BookId} zaktualizowana pomyślnie", book.Id);

        return await GetBookByIdAsync(book.Id);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        _logger.LogInformation("Usuwanie książki ID: {BookId}", id);
        
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            _logger.LogWarning("Próba usunięcia nieistniejącej książki ID: {BookId}", id);
            return false;
        }

        _logger.LogInformation("Usuwanie książki: {Title} (ID: {BookId})", book.Title, book.Id);
        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Książka ID: {BookId} usunięta pomyślnie", id);
        
        return true;
    }

    public async Task<int> DeleteBooksAsync(IEnumerable<int> bookIds)
    {
        var idsList = bookIds.ToList();
        _logger.LogInformation("Masowe usuwanie książek. Liczba ID: {Count}", idsList.Count);
        
        if (!idsList.Any())
        {
            _logger.LogWarning("Próba masowego usunięcia z pustą listą ID");
            return 0;
        }

        // Pobierz książki do usunięcia
        var books = await _context.Books
            .Where(b => idsList.Contains(b.Id))
            .ToListAsync();

        if (!books.Any())
        {
            _logger.LogWarning("Nie znaleziono żadnych książek do usunięcia z podanych ID");
            return 0;
        }

        var foundIds = books.Select(b => b.Id).ToList();
        var notFoundIds = idsList.Except(foundIds).ToList();
        
        if (notFoundIds.Any())
        {
            _logger.LogWarning("Nie znaleziono książek z następującymi ID: {Ids}", string.Join(", ", notFoundIds));
        }

        // Najpierw usuń relacje BookAuthor
        var bookAuthors = await _context.BookAuthors
            .Where(ba => foundIds.Contains(ba.BookId))
            .ToListAsync();

        if (bookAuthors.Any())
        {
            _context.BookAuthors.RemoveRange(bookAuthors);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Usunięto {Count} relacji BookAuthor", bookAuthors.Count);
        }

        // Usuń książki
        _context.Books.RemoveRange(books);
        await _context.SaveChangesAsync();
        
        var titles = string.Join(", ", books.Select(b => b.Title).Take(5));
        if (books.Count > 5)
        {
            titles += $" i {books.Count - 5} więcej";
        }
        
        _logger.LogInformation("Masowo usunięto {Count} książek. Tytuły: {Titles}", books.Count, titles);
        
        return books.Count;
    }

    public async Task<IEnumerable<BookDto>> SearchBooksAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Where(b => b.Title.ToLower().Contains(term) ||
                       b.ISBN.ToLower().Contains(term) ||
                       b.Publisher.ToLower().Contains(term) ||
                       b.Description.ToLower().Contains(term) ||
                       b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(term) ||
                                              ba.Author.LastName.ToLower().Contains(term)))
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Publisher = b.Publisher,
                Year = b.Year,
                Pages = b.Pages,
                Description = b.Description,
                IsAvailable = b.IsAvailable,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                Authors = b.BookAuthors.Select(ba => new AuthorDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName,
                    FullName = ba.Author.FullName,
                    Biography = ba.Author.Biography
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<BookDto>> GetBooksByCategoryAsync(int categoryId)
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Where(b => b.CategoryId == categoryId)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Publisher = b.Publisher,
                Year = b.Year,
                Pages = b.Pages,
                Description = b.Description,
                IsAvailable = b.IsAvailable,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                Authors = b.BookAuthors.Select(ba => new AuthorDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName,
                    FullName = ba.Author.FullName,
                    Biography = ba.Author.Biography
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .Where(b => b.IsAvailable)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                Publisher = b.Publisher,
                Year = b.Year,
                Pages = b.Pages,
                Description = b.Description,
                IsAvailable = b.IsAvailable,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name,
                Authors = b.BookAuthors.Select(ba => new AuthorDto
                {
                    Id = ba.Author.Id,
                    FirstName = ba.Author.FirstName,
                    LastName = ba.Author.LastName,
                    FullName = ba.Author.FullName,
                    Biography = ba.Author.Biography
                }).ToList()
            })
            .ToListAsync();
    }
}

