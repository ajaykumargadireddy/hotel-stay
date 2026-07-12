using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelStay.Domain.Abstractions;
using HotelStay.Domain.Entities;

namespace HotelStay.Infrastructure.Repositories;

public class InMemoryReservationRepository : IReservationRepository
{
    private readonly ConcurrentDictionary<string, Reservation> _reservations = new();

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _reservations.TryAdd(reservation.ReferenceNumber, reservation);
        return Task.CompletedTask;
    }

    public Task<Reservation?> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        // Case-insensitive lookup
        var normalizedRef = referenceNumber.Trim().ToUpperInvariant();

        var match = _reservations.Values
            .FirstOrDefault(r => r.ReferenceNumber.ToUpperInvariant() == normalizedRef);

        return Task.FromResult(match);
    }
}
