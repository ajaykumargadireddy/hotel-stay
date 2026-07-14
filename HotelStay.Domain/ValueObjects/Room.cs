using System;
using HotelStay.Domain.Enums;

namespace HotelStay.Domain.ValueObjects;

public class Room
{
    public Guid RoomId { get; init; }
    public string Provider { get; init; }
    public string Destination { get; init; }
    public string Location { get; init; }
    public RoomType RoomType { get; init; }
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public decimal PerNightRate { get; init; }
    public string Currency { get; init; }
    public string CancellationPolicy { get; init; }
    public string[] Amenities { get; init; }
    public int? StarRating { get; init; }

    public Room()
    {
        Provider = string.Empty;
        Destination = string.Empty;
        Location = string.Empty;
        Currency = string.Empty;
        CancellationPolicy = string.Empty;
        Amenities = Array.Empty<string>();
    }

    public static Room Create(
        Guid roomId,
        string provider,
        string destination,
        string location,
        RoomType roomType,
        DateTime checkIn,
        DateTime checkOut,
        decimal perNightRate,
        string currency,
        string cancellationPolicy,
        string[] amenities,
        int? starRating)
    {
        return new Room
        {
            RoomId = roomId,
            Provider = provider,
            Destination = destination,
            Location = location,
            RoomType = roomType,
            CheckIn = checkIn,
            CheckOut = checkOut,
            PerNightRate = perNightRate,
            Currency = currency,
            CancellationPolicy = cancellationPolicy,
            Amenities = amenities ?? Array.Empty<string>(),
            StarRating = starRating
        };
    }
}
