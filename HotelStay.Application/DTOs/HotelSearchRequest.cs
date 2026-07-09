using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;
using System;

namespace HotelStay.Application.DTOs;

public class HotelSearchRequest
{
    public string Destination { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; } 
    public DateTime CheckOut { get; set; }
    public RoomType? RoomType { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Destination))
        {
            throw new DomainValidationException("Destination cannot be empty");
        }
        if (CheckOut <= CheckIn)
        {
            throw new DomainValidationException("Check-out date must be after check-in date");
        }
    }
}
