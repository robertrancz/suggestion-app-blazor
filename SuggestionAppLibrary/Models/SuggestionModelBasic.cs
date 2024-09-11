namespace SuggestionAppLibrary.Models
{
    public class SuggestionModelBasic
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Suggestion { get; set; }

        public SuggestionModelBasic()
        {}

        public SuggestionModelBasic(SuggestionModel suggestion)
        {
            Id = suggestion.Id;
            Suggestion = suggestion.Suggestion;
        }
    }
}
