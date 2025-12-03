using SystemZarzadzaniaBibliotekaFirmy.DTOs;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<BookDto?> GetBookByIdAsync(int id);
    Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);
    Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto updateBookDto);
    Task<bool> DeleteBookAsync(int id);
    Task<int> DeleteBooksAsync(IEnumerable<int> bookIds);
    Task<IEnumerable<BookDto>> SearchBooksAsync(string searchTerm);
    Task<IEnumerable<BookDto>> GetBooksByCategoryAsync(int categoryId);
    Task<IEnumerable<BookDto>> GetAvailableBooksAsync();
}

