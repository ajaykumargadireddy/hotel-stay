using System;
using HotelStay.Application.DTOs;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Application.Mappers;

public static class ReservationMapper
{
    public static Document ToDocument(DocumentDto dto)
    {
        return Document.Create(dto.HolderName, dto.Type, dto.Number);
    }

    public static ReservationResponse ToResponse(Reservation reservation)
    {
        return new ReservationResponse
        {
            ReferenceNumber = reservation.ReferenceNumber,
            RoomId = reservation.Room.RoomId,
            Destination = reservation.Room.Destination,
            Location = reservation.Room.Location,
            RoomType = reservation.Room.RoomType,
            RoomCheckIn = reservation.Room.CheckIn,
            RoomCheckOut = reservation.Room.CheckOut,
            CheckIn = reservation.CheckIn,
            CheckOut = reservation.CheckOut,
            NumberOfNights = (reservation.CheckOut - reservation.CheckIn).Days,
            TotalPrice = (reservation.CheckOut - reservation.CheckIn).Days * reservation.Room.PerNightRate,
            Currency = reservation.Room.Currency,
            Provider = reservation.Room.Provider,
            Document = new DocumentDto
            {
                HolderName = reservation.Document.HolderName,
                Type = reservation.Document.Type,
                Number = reservation.Document.Number
            },
            ReservationTimestamp = reservation.ReservationTimestamp.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    public static string GenerateReferenceNumber()
    {
        var guid = Guid.NewGuid().ToString("N"); // Remove hyphens
        return $"REF-{guid.Substring(0, 8)}";    // Take first 8 characters
    }
}
