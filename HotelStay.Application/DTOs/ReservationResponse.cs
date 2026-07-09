using System;
using HotelStay.Domain.Enums;

namespace HotelStay.Application.DTOs;

public class ReservationResponse
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public DateTime RoomCheckIn { get; set; }
    public DateTime RoomCheckOut { get; set; }
    public DateTime CheckIn { get; set; } 
    public DateTime CheckOut { get; set; } 
    public int NumberOfNights { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DocumentDto Document { get; set; } = new();
    public string ReservationTimestamp { get; set; } = string.Empty; // Format: yyyy-MM-dd HH:mm:ss
}
