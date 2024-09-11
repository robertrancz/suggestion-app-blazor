namespace SuggestionAppLibrary.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        // The ID coming from Azure Identity
        public string ObjectIdentifier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public List<SuggestionModelBasic> AuthoredSuggestions { get; set; } = new();
        public List<SuggestionModelBasic> VotedOnSuggestions { get; set; } = new();

    }
}
