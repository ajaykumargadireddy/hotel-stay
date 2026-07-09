using System;
using System.Collections.Generic;
using System.Linq;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;
using HotelStay.Application.Mappers;
using HotelStay.Domain.Abstractions;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Exceptions;

namespace HotelStay.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IEnumerable<IHotelProvider> _providers;
    private readonly IReservationRepository _repository;

    public ReservationService(IEnumerable<IHotelProvider> providers, IReservationRepository repository)
    {
        _providers = providers;
        _repository = repository;
    }

    public string Reserve(ReservationRequest request)
    {
        // 1. Fetch Room by roomId from providers (query all until found)
        var room = _providers
            .Select(p => p.GetRoomById(request.RoomId))
            .FirstOrDefault(r => r != null);

        if (room == null)
        {
            throw new DomainValidationException("Invalid roomId");
        }

        // 2. Prepare value objects via mapper
        var document = ReservationMapper.ToDocument(request.Document);

        // 3. Call domain factory (service responsibility, not mapper)
        var referenceNumber = ReservationMapper.GenerateReferenceNumber();
        var reservation = Reservation.Reserve(
            referenceNumber: referenceNumber,
            roomId: request.RoomId,
            room: room,
            checkIn: request.CheckIn,
            checkOut: request.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now
        );

        // 4. Persist and return
        _repository.Add(reservation);
        return referenceNumber;
    }

    public ReservationResponse GetByReference(string referenceNumber)
    {
        var reservation = _repository.GetByReference(referenceNumber);
        
        if (reservation == null)
        {
            throw new ReservationNotFoundException("Reservation not found");
        }

        return ReservationMapper.ToResponse(reservation);
    }
}
