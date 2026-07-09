using System;
using System.Collections.Generic;
using System.Linq;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Infrastructure.Providers.BudgetNests;

public class BudgetNestsProvider : IHotelProvider
{
    public IEnumerable<Room> Search(HotelSearchRequest request)
    {
        var matchingRooms = BudgetNestsStubData.Catalog.Values
            .Where(entry =>
                entry.available &&  // Filter out unavailable rooms
                entry.destination.Equals(request.Destination, StringComparison.OrdinalIgnoreCase) &&
                request.CheckIn >= entry.check_in &&
                request.CheckOut <= entry.check_out &&
                (!request.RoomType.HasValue || entry.room_type == request.RoomType))
            .Select(BudgetNestsMapper.ToRoom)
            .ToList();

        return matchingRooms;
    }

    public Room? GetRoomById(Guid roomId)
    {
        if (BudgetNestsStubData.Catalog.TryGetValue(roomId, out var entry))
        {
            return BudgetNestsMapper.ToRoom(entry);
        }

        return null;
    }
}
