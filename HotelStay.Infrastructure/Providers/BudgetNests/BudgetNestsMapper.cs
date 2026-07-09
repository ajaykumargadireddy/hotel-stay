using System;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Infrastructure.Providers.BudgetNests;

public static class BudgetNestsMapper
{
    public static Room ToRoom(BudgetNestsRoomEntry entry)
    {
        return Room.Create(
            roomId: entry.room_id,
            provider: entry.provider,
            destination: entry.destination,
            location: entry.location,
            roomType: entry.room_type,
            checkIn: entry.check_in,
            checkOut: entry.check_out,
            perNightRate: entry.per_night_rate,
            currency: entry.currency,
            cancellationPolicy: entry.cancellation_policy,
            amenities: entry.amenities,
            starRating: entry.star_rating
        );
    }
}
