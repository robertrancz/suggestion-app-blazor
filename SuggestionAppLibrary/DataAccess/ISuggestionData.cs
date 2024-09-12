
namespace SuggestionAppLibrary.DataAccess
{
    public interface ISuggestionData
    {
        Task CreateSuggestionAsync(SuggestionModel suggestion);
        Task<List<SuggestionModel>> GetAllApprovedSuggestionsAsync();
        Task<List<SuggestionModel>> GetAllSuggestionsAsync();
        Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApprovalAsync();
        Task<SuggestionModel> GetSuggestionAsync(string id);
        Task<List<SuggestionModel>> GetUserSuggestionsAsync(string userId);
        Task UpdateSuggestionAsync(SuggestionModel suggestion);
        Task UpvoteSuggestionAsync(string suggestionId, string userId);
    }
}