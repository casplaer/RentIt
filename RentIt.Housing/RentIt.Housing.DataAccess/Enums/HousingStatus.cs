using System.Text.Json.Serialization;

namespace RentIt.Housing.DataAccess.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HousingStatus
    {
        Available,
        Booked,
        Unpublished,
        Rejected
    }
}