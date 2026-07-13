namespace HotelStay.Application.DTOs;

/// <summary>
/// Response returned when a reservation is successfully created.
/// </summary>
public class ReservationCreatedResponse
{
    public string ReferenceNumber { get; set; } = string.Empty;
}
