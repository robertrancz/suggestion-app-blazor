
namespace SuggestionAppLibrary.DataAccess
{
    public interface IStatusData
    {
        Task CreateStatusAsync(StatusModel status);
        Task<List<StatusModel>> GetAllStatusesAsync();
    }
}