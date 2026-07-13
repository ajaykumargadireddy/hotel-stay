using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task<ReservationCreatedResponse> ReserveAsync(ReservationRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Fetch Room by roomId from providers (query all in parallel until found)
        var roomTasks = _providers.Select(p => p.GetRoomByIdAsync(request.RoomId, cancellationToken));
        var rooms = await Task.WhenAll(roomTasks);
        var room = rooms.FirstOrDefault(r => r != null);

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
        await _repository.AddAsync(reservation, cancellationToken);
        return new ReservationCreatedResponse { ReferenceNumber = referenceNumber };
    }

    public async Task<ReservationResponse> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        var reservation = await _repository.GetByReferenceAsync(referenceNumber, cancellationToken);

        if (reservation == null)
        {
            throw new ReservationNotFoundException("Reservation not found");
        }

        return ReservationMapper.ToResponse(reservation);
    }
}
