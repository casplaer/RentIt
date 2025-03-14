using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RentIt.Housing.DataAccess.Entities
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid ReviewId { get; set; } 
        public Guid HousingId { get; set; } 
        public Guid UserId { get; set; } 
        public int Rating { get; set; } 
        public string Comment { get; set; } 
        public DateTime CreatedAt { get; set; } 
    }
}
