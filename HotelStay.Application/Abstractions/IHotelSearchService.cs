using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelStay.Application.DTOs;

namespace HotelStay.Application.Abstractions;

public interface IHotelSearchService
{
    Task<IEnumerable<HotelSearchResponse>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken = default);
}
