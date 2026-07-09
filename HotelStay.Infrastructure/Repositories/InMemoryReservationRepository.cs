using System.Collections.Concurrent;
using System.Linq;
using HotelStay.Domain.Abstractions;
using HotelStay.Domain.Entities;

namespace HotelStay.Infrastructure.Repositories;

public class InMemoryReservationRepository : IReservationRepository
{
    private readonly ConcurrentDictionary<string, Reservation> _reservations = new();

    public void Add(Reservation reservation)
    {
        _reservations.TryAdd(reservation.ReferenceNumber, reservation);
    }

    public Reservation? GetByReference(string referenceNumber)
    {
        // Case-insensitive lookup
        var normalizedRef = referenceNumber.Trim().ToUpperInvariant();
        
        var match = _reservations.Values
            .FirstOrDefault(r => r.ReferenceNumber.ToUpperInvariant() == normalizedRef);

        return match;
    }
}
