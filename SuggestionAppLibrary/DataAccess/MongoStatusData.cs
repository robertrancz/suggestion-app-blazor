using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;

namespace SuggestionAppLibrary.DataAccess
{
    public class MongoStatusData : IStatusData
    {
        private readonly IMongoCollection<StatusModel> _statuses;
        private readonly IMemoryCache _cache;
        private const string CacheName = "StatusData";

        public MongoStatusData(IDbConnection db, IMemoryCache cache)
        {
            _cache = cache;
            _statuses = db.StatusCollection;
        }

        public async Task<List<StatusModel>> GetAllStatusesAsync()
        {
            var output = _cache.Get<List<StatusModel>>(CacheName);

            if (output is null)
            {
                var result = await _statuses.FindAsync(_ => true);
                output = result.ToList();

                _cache.Set(CacheName, output, TimeSpan.FromDays(1));
            }

            return output;
        }

        public Task CreateStatusAsync(StatusModel status)
        {
            return _statuses.InsertOneAsync(status);
        }
    }
}
