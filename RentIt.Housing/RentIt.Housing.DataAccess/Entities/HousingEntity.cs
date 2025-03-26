using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using RentIt.Housing.DataAccess.Enums;

namespace RentIt.Housing.DataAccess.Entities
{
    public class HousingEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid HousingId { get; set; }
        public Guid OwnerId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }       
        public string Address { get; set; }          
        public string City { get; set; }            
        public string Country { get; set; }           
        public decimal PricePerNight { get; set; }
        public int NumberOfRooms { get; set; }
        public double Rating { get; set; }             

        public List<string> Amenities { get; set; } = new();  

        public HousingStatus Status { get; set; }

        public List<HousingImage> Images { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();

        public DateOnly? EstimatedEndDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
