using System;

namespace HotelStay.Application.DTOs;

public class ReservationRequest
{
    public Guid RoomId { get; set; }

    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public DocumentDto Document { get; set; } = new();
}
