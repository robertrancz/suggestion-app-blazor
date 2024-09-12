using Microsoft.Extensions.Caching.Memory;

namespace SuggestionAppLibrary.DataAccess
{
    public class MongoSuggestionData : ISuggestionData
    {
        private readonly IDbConnection _db;
        private readonly IUserData _userData;
        private readonly IMemoryCache _cache;
        private readonly IMongoCollection<SuggestionModel> _suggestions;
        private const string CacheName = "SuggestionData";

        public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
        {
            _db = db;
            _userData = userData;
            _cache = cache;
            _suggestions = db.SuggestionCollection;
        }

        public async Task<List<SuggestionModel>> GetAllSuggestionsAsync()
        {
            var output = _cache.Get<List<SuggestionModel>>(CacheName);
            if (output is null)
            {
                var result = await _suggestions.FindAsync(_ => true);
                output = result.ToList();

                _cache.Set(CacheName, output, TimeSpan.FromMinutes(1));
            }

            return output;
        }

        public async Task<List<SuggestionModel>> GetUserSuggestionsAsync(string userId)
        {
            var output = _cache.Get<List<SuggestionModel>>(userId);
            if (output is null)
            {
                var result = await _suggestions.FindAsync(s => s.Author.Id == userId);
                output = result.ToList();

                _cache.Set(userId, output, TimeSpan.FromMinutes(1));
            }
            return output;
        }

        public async Task<List<SuggestionModel>> GetAllApprovedSuggestionsAsync()
        {
            var output = await GetAllSuggestionsAsync();
            return output.Where(s => s.ApprovedForRelease).ToList();
        }

        public async Task<SuggestionModel> GetSuggestionAsync(string id)
        {
            var result = await _suggestions.FindAsync(s => s.Id == id);
            return result.FirstOrDefault();
        }

        public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApprovalAsync()
        {
            var output = await GetAllSuggestionsAsync();
            return output.Where(s => s.ApprovedForRelease == false && s.Rejected == false).ToList();
        }

        public async Task UpdateSuggestionAsync(SuggestionModel suggestion)
        {
            await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
            _cache.Remove(CacheName);
        }

        public async Task UpvoteSuggestionAsync(string suggestionId, string userId)
        {
            var client = _db.Client;

            using var session = await client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.StatusCollectionName);
                var suggestion = (await suggestionsInTransaction.FindAsync(s => s.Id == suggestionId)).First();

                bool isUpvote = suggestion.UserVotes.Add(userId);
                if (isUpvote == false)
                {
                    suggestion.UserVotes.Remove(userId);
                }

                await suggestionsInTransaction.ReplaceOneAsync(s => s.Id == suggestionId, suggestion);

                var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                var user = await _userData.GetUserAsync(suggestion.Author.Id);

                if (isUpvote)
                {
                    user.VotedOnSuggestions.Add(new SuggestionModelBasic(suggestion));
                }
                else
                {
                    var suggestionToRemove = user.VotedOnSuggestions.Where(s => s.Id == suggestionId).First();
                    user.VotedOnSuggestions.Remove(suggestionToRemove);
                }

                await usersInTransaction.ReplaceOneAsync(u => u.Id == userId, user);

                await session.CommitTransactionAsync();

                _cache.Remove(CacheName);
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task CreateSuggestionAsync(SuggestionModel suggestion)
        {
            var client = _db.Client;
            using var session = await client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_db.DbName);
                var suggestionsInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
                await suggestionsInTransaction.InsertOneAsync(suggestion);
                var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
                var user = await _userData.GetUserAsync(suggestion.Author.Id);
                user.AuthoredSuggestions.Add(new SuggestionModelBasic(suggestion));
                await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }
    }
}
