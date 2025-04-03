using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RentIt.Housing.DataAccess.Entities
{
    public class HousingImage
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid ImageId { get; set; }           
        public Guid HousingId { get; set; }       
        public string ImageUrl { get; set; }          
        public int Order { get; set; }       
    }
}
