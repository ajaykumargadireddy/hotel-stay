using System;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Infrastructure.Providers.PremierStays;

public static class PremierStaysMapper
{
    public static Room ToRoom(PremierStaysRoomEntry entry)
    {
        return Room.Create(
            roomId: entry.RoomId,
            provider: entry.Provider,
            destination: entry.Destination,
            location: entry.Location,
            roomType: entry.RoomType,
            checkIn: entry.CheckIn,
            checkOut: entry.CheckOut,
            perNightRate: entry.PerNightRate,
            currency: entry.Currency,
            cancellationPolicy: entry.CancellationPolicy,
            amenities: entry.Amenities,
            starRating: entry.StarRating
        );
    }
}
