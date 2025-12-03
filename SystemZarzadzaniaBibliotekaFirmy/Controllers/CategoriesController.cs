using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SystemZarzadzaniaBibliotekaFirmy.DTOs;
using SystemZarzadzaniaBibliotekaFirmy.Services;

namespace SystemZarzadzaniaBibliotekaFirmy.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    private string GetCurrentUsername() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    private string GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) tworzy nową kategorię: {Name}", 
            username, userId, createCategoryDto.Name);
        
        var category = await _categoryService.CreateCategoryAsync(createCategoryDto);
        
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) utworzył kategorię ID: {CategoryId}, Nazwa: {Name}", 
            username, userId, category.Id, category.Name);
        
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, UpdateCategoryDto updateCategoryDto)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) aktualizuje kategorię ID: {CategoryId}", 
            username, userId, id);
        
        var category = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
        if (category == null)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował zaktualizować nieistniejącą kategorię ID: {CategoryId}", 
                username, userId, id);
            return NotFound();
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) zaktualizował kategorię ID: {CategoryId}, Nazwa: {Name}", 
            username, userId, category.Id, category.Name);

        return Ok(category);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var username = GetCurrentUsername();
        var userId = GetCurrentUserId();
        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) usuwa kategorię ID: {CategoryId}", 
            username, userId, id);
        
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (!result)
        {
            _logger.LogWarning("Użytkownik {Username} (ID: {UserId}) próbował usunąć nieistniejącą kategorię ID: {CategoryId}", 
                username, userId, id);
            return NotFound();
        }

        _logger.LogInformation("Użytkownik {Username} (ID: {UserId}) usunął kategorię ID: {CategoryId}", 
            username, userId, id);

        return NoContent();
    }
}

