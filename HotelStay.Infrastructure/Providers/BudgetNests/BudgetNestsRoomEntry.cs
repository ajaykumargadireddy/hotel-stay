using System;
using HotelStay.Domain.Enums;

namespace HotelStay.Infrastructure.Providers.BudgetNests;

/// <summary>
/// BudgetNests returns room data in snake_case JSON format.
/// This C# model mirrors that format using snake_case field names to represent the origin JSON contract.
/// </summary>
public class BudgetNestsRoomEntry
{
    public Guid room_id { get; set; }
    public string provider { get; set; } = string.Empty;
    public string destination { get; set; } = string.Empty;
    public string location { get; set; } = string.Empty;
    public RoomType room_type { get; set; }
    public DateTime check_in { get; set; }
    public DateTime check_out { get; set; }
    public decimal per_night_rate { get; set; }
    public string currency { get; set; } = string.Empty;
    public string cancellation_policy { get; set; } = string.Empty;
    public string[] amenities { get; set; } = Array.Empty<string>();
    public int? star_rating { get; set; }

    public bool available {  get; set; } // This might availability status of the room, true if available, false otherwise.
}
