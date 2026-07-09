using System;
using System.Collections.Generic;
using System.Linq;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Infrastructure.Providers.PremierStays;

public class PremierStaysProvider : IHotelProvider
{
    public IEnumerable<Room> Search(HotelSearchRequest request)
    {
        var matchingRooms = PremierStaysStubData.Catalog.Values
            .Where(entry =>
                entry.Destination.Equals(request.Destination, StringComparison.OrdinalIgnoreCase) &&
                request.CheckIn >= entry.CheckIn &&
                request.CheckOut <= entry.CheckOut &&
                (!request.RoomType.HasValue || entry.RoomType == request.RoomType))
            .Select(PremierStaysMapper.ToRoom)
            .ToList();

        return matchingRooms;
    }

    public Room? GetRoomById(Guid roomId)
    {
        if (PremierStaysStubData.Catalog.TryGetValue(roomId, out var entry))
        {
            return PremierStaysMapper.ToRoom(entry);
        }

        return null;
    }
}
