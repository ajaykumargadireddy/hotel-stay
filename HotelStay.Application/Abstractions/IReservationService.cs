using HotelStay.Application.DTOs;

namespace HotelStay.Application.Abstractions;

public interface IReservationService
{
    string Reserve(ReservationRequest request);
    ReservationResponse GetByReference(string referenceNumber);
}
