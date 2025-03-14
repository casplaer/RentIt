using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RentIt.Housing.DataAccess.Entities
{
    public class Availability
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid AvailabilityId { get; set; }
        public Guid HousingId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}