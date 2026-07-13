using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;

namespace HotelStay.Application.Services;

public class HotelSearchService : IHotelSearchService
{
    private readonly IEnumerable<IHotelProvider> _providers;

    public HotelSearchService(IEnumerable<IHotelProvider> providers)
    {
        _providers = providers;
    }

    public async Task<IEnumerable<HotelSearchResponse>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken = default)
    {
        request.Validate();

        // Query all providers in parallel and aggregate results
        var searchTasks = _providers.Select(provider => provider.SearchAsync(request, cancellationToken));
        var results = await Task.WhenAll(searchTasks);

        var rooms = results
            .SelectMany(r => r)
            .OrderBy(x => x.PerNightRate);

        return rooms.Select(room => HotelSearchResponse.Create(room, request.CheckIn, request.CheckOut));
    }

    public async Task<RoomDetailsResponse?> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        // Query all providers in parallel until room is found
        var roomTasks = _providers.Select(p => p.GetRoomByIdAsync(roomId, cancellationToken));
        var rooms = await Task.WhenAll(roomTasks);
        var room = rooms.FirstOrDefault(r => r != null);

        if (room == null)
        {
            return null;
        }

        return new RoomDetailsResponse
        {
            RoomId = room.RoomId,
            Provider = room.Provider,
            Destination = room.Destination,
            Location = room.Location,
            RoomType = room.RoomType.ToString(),
            PerNightRate = room.PerNightRate,
            Currency = room.Currency,
            CancellationPolicy = room.CancellationPolicy,
            Amenities = room.Amenities.ToArray(),
            StarRating = room.StarRating
        };
    }
}
