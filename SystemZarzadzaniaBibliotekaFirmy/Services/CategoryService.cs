using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SystemZarzadzaniaBibliotekaFirmy.Data;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Models;

namespace SystemZarzadzaniaBibliotekaFirmy.Services;

public class CategoryService : ICategoryService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(LibraryDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        _logger.LogInformation("Tworzenie nowej kategorii: {Name}", createCategoryDto.Name);
        
        var category = new Category
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Kategoria utworzona pomyślnie. ID: {CategoryId}, Nazwa: {Name}", 
            category.Id, category.Name);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
    {
        _logger.LogInformation("Aktualizacja kategorii ID: {CategoryId}", id);
        
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Próba aktualizacji nieistniejącej kategorii ID: {CategoryId}", id);
            return null;
        }

        _logger.LogInformation("Aktualizacja kategorii: {OldName} -> {NewName}", 
            category.Name, updateCategoryDto.Name);

        category.Name = updateCategoryDto.Name;
        category.Description = updateCategoryDto.Description;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Kategoria ID: {CategoryId} zaktualizowana pomyślnie", id);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        _logger.LogInformation("Usuwanie kategorii ID: {CategoryId}", id);
        
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            _logger.LogWarning("Próba usunięcia nieistniejącej kategorii ID: {CategoryId}", id);
            return false;
        }

        _logger.LogInformation("Usuwanie kategorii: {Name} (ID: {CategoryId})", category.Name, category.Id);
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Kategoria ID: {CategoryId} usunięta pomyślnie", id);
        
        return true;
    }
}

