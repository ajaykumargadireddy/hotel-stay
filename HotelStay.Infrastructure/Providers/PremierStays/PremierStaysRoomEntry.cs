using System;
using HotelStay.Domain.Enums;

namespace HotelStay.Infrastructure.Providers.PremierStays;

/// <summary>
/// PremierStays returns room data in PascalCase JSON format.
/// This C# model mirrors that format directly.
/// </summary>
public class PremierStaysRoomEntry
{
    public Guid RoomId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal PerNightRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CancellationPolicy { get; set; } = string.Empty;
    public string[] Amenities { get; set; } = Array.Empty<string>();
    public int? StarRating { get; set; }
}
