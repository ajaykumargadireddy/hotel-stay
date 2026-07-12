using System.Threading;
using System.Threading.Tasks;
using HotelStay.Domain.Entities;

namespace HotelStay.Domain.Abstractions;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<Reservation?> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default);
}
