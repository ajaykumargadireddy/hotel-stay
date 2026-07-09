using HotelStay.Domain.Entities;

namespace HotelStay.Domain.Abstractions;

public interface IReservationRepository
{
    void Add(Reservation reservation);
    Reservation? GetByReference(string referenceNumber);
}
