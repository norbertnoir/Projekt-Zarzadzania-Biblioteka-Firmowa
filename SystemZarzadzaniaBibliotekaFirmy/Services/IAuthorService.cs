using SystemZarzadzaniaBibliotekaFirmy.DTOs;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public interface IAuthorService
{
    Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync();
    Task<AuthorDto?> GetAuthorByIdAsync(int id);
    Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto);
    Task<AuthorDto?> UpdateAuthorAsync(int id, UpdateAuthorDto updateAuthorDto);
    Task<bool> DeleteAuthorAsync(int id);
}

