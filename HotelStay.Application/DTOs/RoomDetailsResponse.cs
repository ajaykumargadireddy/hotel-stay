using System;

namespace HotelStay.Application.DTOs;

/// <summary>
/// Response containing room details for reservation page.
/// </summary>
public class RoomDetailsResponse
{
    public Guid RoomId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PerNightRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CancellationPolicy { get; set; } = string.Empty;
    public string[] Amenities { get; set; } = Array.Empty<string>();
    public int? StarRating { get; set; }
}
