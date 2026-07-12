using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelStay.Application.DTOs;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Application.Abstractions;

public interface IHotelProvider
{
    Task<IEnumerable<Room>> SearchAsync(HotelSearchRequest request, CancellationToken cancellationToken = default);
    Task<Room?> GetRoomByIdAsync(Guid roomId, CancellationToken cancellationToken = default);
}
