using System;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Application.DTOs;

public class HotelSearchResponse
{
    public Room Room { get; init; } = null!;
    public int TotalNights { get; init; }
    public decimal TotalPrice { get; init; }

    public static HotelSearchResponse Create(Room room, DateTime checkIn, DateTime checkOut)
    {
        var totalNights = (checkOut - checkIn).Days;
        return new HotelSearchResponse
        {
            Room = room,
            TotalNights = totalNights,
            TotalPrice = room.PerNightRate * totalNights
        };
    }
}
