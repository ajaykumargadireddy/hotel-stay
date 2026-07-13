using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Infrastructure.Providers.PremierStays;

public class PremierStaysProvider : IHotelProvider
{
    public Task<IEnumerable<Room>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken = default)
    {
        var matchingRooms = PremierStaysStubData.Catalog.Values
            .Where(entry =>
                entry.Destination.Equals(request.Destination, StringComparison.OrdinalIgnoreCase) &&
                request.CheckIn >= entry.CheckIn &&
                request.CheckOut <= entry.CheckOut &&
                (!request.RoomType.HasValue || entry.RoomType == request.RoomType))
            .Select(PremierStaysMapper.ToRoom)
            .ToList();

        return Task.FromResult<IEnumerable<Room>>(matchingRooms);
    }

    public Task<Room?> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        if (PremierStaysStubData.Catalog.TryGetValue(roomId, out var entry))
        {
            return Task.FromResult<Room?>(PremierStaysMapper.ToRoom(entry));
        }

        return Task.FromResult<Room?>(null);
    }
}
