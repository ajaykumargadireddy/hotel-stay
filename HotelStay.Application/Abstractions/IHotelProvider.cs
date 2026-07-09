using System;
using System.Collections.Generic;
using HotelStay.Application.DTOs;
using HotelStay.Domain.ValueObjects;

namespace HotelStay.Application.Abstractions;

public interface IHotelProvider
{
    IEnumerable<Room> Search(HotelSearchRequest request);
    Room? GetRoomById(Guid roomId);
}
