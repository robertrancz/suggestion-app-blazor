namespace SuggestionAppLibrary.Models
{
    public class UserModelBasic
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string DisplayName { get; set; }

        public UserModelBasic()
        {}

        public UserModelBasic(UserModel user)
        {
            Id = user.Id;
            DisplayName = user.DisplayName;
        }
    }
}
