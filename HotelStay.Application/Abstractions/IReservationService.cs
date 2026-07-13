using System.Threading;
using System.Threading.Tasks;
using HotelStay.Application.DTOs;

namespace HotelStay.Application.Abstractions;

public interface IReservationService
{
    Task<ReservationCreatedResponse> ReserveAsync(ReservationRequest request, CancellationToken cancellationToken = default);
    Task<ReservationResponse> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default);
}
