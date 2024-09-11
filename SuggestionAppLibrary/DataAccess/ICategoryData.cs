
namespace SuggestionAppLibrary.DataAccess
{
    public interface ICategoryData
    {
        Task CreateCategoryAsync(CategoryModel category);
        Task<List<CategoryModel>> GetAllCategoriesAsync();
    }
}