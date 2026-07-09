using HotelStay.Application.DTOs;

namespace HotelStay.Application.Abstractions;

public interface IReservationService
{
    ReservationResponse Reserve(ReservationRequest request);
    ReservationResponse GetByReference(string referenceNumber);
}
