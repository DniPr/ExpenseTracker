using ExpenseTracker.DTOs.Categories;

namespace ExpenseTracker.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(string userId);

    Task<CategoryDto?> GetByIdAsync(int id, string userId);

    Task<CategoryDto?> CreateAsync(CategoryCreateDto dto,string userId);

    Task<bool> UpdateAsync(int id,CategoryUpdateDto dto,string userId);

    Task<bool> DeleteAsync(int id,string userId);
}