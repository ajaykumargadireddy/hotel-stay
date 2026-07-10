using System.Text.Json.Serialization;

namespace HotelStay.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoomType
{
    Standard,
    Deluxe,
    Suite
}
