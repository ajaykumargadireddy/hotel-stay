using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Infrastructure.Providers.BudgetNests;

public class BudgetNestsProvider : IHotelProvider
{
    public Task<IEnumerable<Room>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var matchingRooms = BudgetNestsStubData.Catalog.Values
            .Where(entry =>
                entry.available &&  // Filter out unavailable rooms
                entry.destination.Equals(request.Destination, StringComparison.OrdinalIgnoreCase) &&
                request.CheckIn >= entry.check_in &&
                request.CheckOut <= entry.check_out &&
                (!request.RoomType.HasValue || entry.room_type == request.RoomType))
            .Select(BudgetNestsMapper.ToRoom)
            .ToList();

        return Task.FromResult<IEnumerable<Room>>(matchingRooms);
    }

    public Task<Room?> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (BudgetNestsStubData.Catalog.TryGetValue(roomId, out var entry))
        {
            return Task.FromResult<Room?>(BudgetNestsMapper.ToRoom(entry));
        }

        return Task.FromResult<Room?>(null);
    }
}
