using System;

namespace HotelStay.Domain.Exceptions;

public sealed class ReservationNotFoundException : Exception
{
    public ReservationNotFoundException(string message) : base(message) { }

    public ReservationNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
