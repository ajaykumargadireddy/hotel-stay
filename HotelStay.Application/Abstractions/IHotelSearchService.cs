using System.Collections.Generic;
using HotelStay.Application.DTOs;

namespace HotelStay.Application.Abstractions;

public interface IHotelSearchService
{
    IEnumerable<HotelSearchResponse> Search(HotelSearchRequest request);
}
