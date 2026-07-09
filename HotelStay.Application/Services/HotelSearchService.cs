using System.Collections.Generic;
using System.Linq;
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

    public IEnumerable<HotelSearchResponse> Search(HotelSearchRequest request)
    {
        request.Validate();
        // Query all providers in parallel and aggregate results
        var rooms = _providers
            .AsParallel()
            .SelectMany(provider => provider.Search(request))
            .ToList().OrderBy(x => x.PerNightRate);

        return rooms.Select(room => HotelSearchResponse.Create(room, request.CheckIn, request.CheckOut));
    }
}
