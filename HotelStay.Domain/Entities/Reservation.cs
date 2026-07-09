using System;
using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Domain.Entities;

public class Reservation
{
    public string ReferenceNumber { get; private set; }
    public Room Room { get; private set; }
    public Document Document { get; private set; }
    public DateTime ReservationTimestamp { get; private set; }

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    // Computed properties based on reservation dates
    public int NumberOfNights => (CheckOut - CheckIn).Days;
    public decimal TotalPrice => Room.PerNightRate * NumberOfNights;

    private Reservation()
    {
        ReferenceNumber = string.Empty;
        Room = null!;
        Document = null!;
    }

    public static Reservation Reserve(
        string referenceNumber,
        Guid roomId,
        Room room,
        DateTime checkIn,
        DateTime checkOut,
        Document document,
        DateTime reservationTimestamp)
    {
        // Validate document type matches location (India = domestic, non-India = international)
        bool isInternational = !room.Location.Equals("India", StringComparison.OrdinalIgnoreCase);
        if (isInternational && document.Type != DocumentType.Passport)
        {
            throw new DocumentMismatchException("International destinations require Passport");
        }

        // Validate date range (check-out > check-in)
        if (checkOut <= checkIn)
        {
            throw new DomainValidationException("Check-out must be after check-in");
        }

        if(!(checkIn >= room.CheckIn && checkIn<= room.CheckOut))
        {
            throw new DomainValidationException("Check-in date is outside the room's availability");
        }

        if(!(checkOut >= room.CheckIn && checkOut <= room.CheckOut))
        {
            throw new DomainValidationException("Check-out date is outside the room's availability");
        }

        // Validate document not empty
        if (string.IsNullOrWhiteSpace(document.Number))
        {
            throw new DomainValidationException("Document number is required");
        }

        return new Reservation
        {
            ReferenceNumber = referenceNumber,
            Room = room,
            Document = document,
            CheckOut = checkOut,
            CheckIn = checkIn,
            ReservationTimestamp = reservationTimestamp
        };
    }
}
