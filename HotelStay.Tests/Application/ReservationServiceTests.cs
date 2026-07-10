using System;
using System.Collections.Generic;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;
using HotelStay.Application.Services;
using HotelStay.Domain.Abstractions;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;
using HotelStay.Domain.ValueObjects;
using Moq;
using Xunit;

namespace HotelStay.Tests.Application;

public class ReservationServiceTests
{
    private Room CreateTestRoom(Guid roomId, string location = "India")
    {
        return Room.Create(
            roomId: roomId,
            provider: "TestProvider",
            destination: "BOM",
            location: location,
            roomType: RoomType.Standard,
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-05"),
            perNightRate: 3000m,
            currency: "INR",
            cancellationPolicy: "FreeCancellation",
            amenities: new[] { "WiFi" },
            starRating: 3
        );
    }

    [Fact]
    public void Reserve_ShouldCallGetRoomByIdOnProvidersUntilFound()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = CreateTestRoom(roomId);

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.GetRoomById(roomId)).Returns((Room?)null);

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.GetRoomById(roomId)).Returns(room);

        var mockProvider3 = new Mock<IHotelProvider>();
        // Should not be called since room is found in provider2

        var mockRepository = new Mock<IReservationRepository>();
        mockRepository.Setup(r => r.Add(It.IsAny<Reservation>()));

        var providers = new List<IHotelProvider> { mockProvider1.Object, mockProvider2.Object, mockProvider3.Object };
        var service = new ReservationService(providers, mockRepository.Object);

        var request = new ReservationRequest
        {
            RoomId = roomId,
            CheckIn = DateTime.Parse("2024-04-01"),
            CheckOut = DateTime.Parse("2024-04-05"),
            Document = new DocumentDto
            {
                HolderName = "John Doe",
                Type = DocumentType.Passport,
                Number = "P123456789"
            }
        };

        // Act
        var response = service.Reserve(request);

        // Assert
        Assert.NotNull(response);
        mockProvider1.Verify(p => p.GetRoomById(roomId), Times.Once);
        mockProvider2.Verify(p => p.GetRoomById(roomId), Times.Once);
        mockProvider3.Verify(p => p.GetRoomById(roomId), Times.Never);
    }

    [Fact]
    public void Reserve_WhenRoomIdNotFoundInAnyProvider_ShouldThrowDomainValidationException()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.GetRoomById(roomId)).Returns((Room?)null);

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.GetRoomById(roomId)).Returns((Room?)null);

        var mockRepository = new Mock<IReservationRepository>();

        var providers = new List<IHotelProvider> { mockProvider1.Object, mockProvider2.Object };
        var service = new ReservationService(providers, mockRepository.Object);

        var request = new ReservationRequest
        {
            RoomId = roomId,
            CheckIn = DateTime.Parse("2024-04-01"),
            CheckOut = DateTime.Parse("2024-04-05"),
            Document = new DocumentDto
            {
                HolderName = "John Doe",
                Type = DocumentType.Passport,
                Number = "P123456789"
            }
        };

        // Act & Assert
        var exception = Assert.Throws<DomainValidationException>(() => service.Reserve(request));
    }

    [Fact]
    public void Reserve_ShouldGenerateReferenceNumberWithCorrectFormat()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = CreateTestRoom(roomId);

        var mockProvider = new Mock<IHotelProvider>();
        mockProvider.Setup(p => p.GetRoomById(roomId)).Returns(room);

        var mockRepository = new Mock<IReservationRepository>();
        mockRepository.Setup(r => r.Add(It.IsAny<Reservation>()));

        var providers = new List<IHotelProvider> { mockProvider.Object };
        var service = new ReservationService(providers, mockRepository.Object);

        var request = new ReservationRequest
        {
            RoomId = roomId,
            CheckIn = room.CheckIn,
            CheckOut = room.CheckOut,
            Document = new DocumentDto
            {
                HolderName = "John Doe",
                Type = DocumentType.Passport,
                Number = "P123456789"
            }
        };

        // Act
        var response = service.Reserve(request);

        // Assert
        Assert.NotNull(response);
        Assert.StartsWith("REF-", response);
        Assert.Equal(12, response.Length); // REF- + 8 hex chars
        Assert.Matches(@"^REF-[a-f0-9]{8}$", response);
    }

    [Fact]
    public void Reserve_ShouldPersistReservationViaRepository()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = CreateTestRoom(roomId);

        var mockProvider = new Mock<IHotelProvider>();
        mockProvider.Setup(p => p.GetRoomById(roomId)).Returns(room);

        var mockRepository = new Mock<IReservationRepository>();
        Reservation? capturedReservation = null;
        mockRepository.Setup(r => r.Add(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => capturedReservation = r);

        var providers = new List<IHotelProvider> { mockProvider.Object };
        var service = new ReservationService(providers, mockRepository.Object);

        var request = new ReservationRequest
        {
            RoomId = roomId,
            CheckIn = room.CheckIn,
            CheckOut = room.CheckOut,
            Document = new DocumentDto
            {
                HolderName = "Jane Smith",
                Type = DocumentType.Passport,
                Number = "P987654321"
            }
        };

        // Act
        var response = service.Reserve(request);

        // Assert
        mockRepository.Verify(r => r.Add(It.IsAny<Reservation>()), Times.Once);
        Assert.NotNull(capturedReservation);
        Assert.Equal(room, capturedReservation.Room);
        Assert.Equal("Jane Smith", capturedReservation.Document.HolderName);
        Assert.Equal(DocumentType.Passport, capturedReservation.Document.Type);
        Assert.Equal("P987654321", capturedReservation.Document.Number);
    }

    [Fact]
    public void GetByReference_ShouldRetrieveReservationSuccessfully()
    {
        // Arrange
        var referenceNumber = "REF-12345678";
        var room = CreateTestRoom(Guid.NewGuid());
        var document = Document.Create("John Doe", DocumentType.Passport, "P123456789");
        var reservation = Reservation.Reserve(
            referenceNumber: referenceNumber,
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now);

        var mockRepository = new Mock<IReservationRepository>();
        mockRepository.Setup(r => r.GetByReference(referenceNumber)).Returns(reservation);

        var mockProvider = new Mock<IHotelProvider>();
        var providers = new List<IHotelProvider> { mockProvider.Object };
        var service = new ReservationService(providers, mockRepository.Object);

        // Act
        var response = service.GetByReference(referenceNumber);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(reservation.ReferenceNumber, response.ReferenceNumber);
        Assert.Equal(room.RoomId, response.RoomId);
        Assert.Equal("John Doe", response.Document.HolderName);
        mockRepository.Verify(r => r.GetByReference(referenceNumber), Times.Once);
    }

    [Fact]
    public void GetByReference_WhenNotFound_ShouldThrowReservationNotFoundException()
    {
        // Arrange
        var referenceNumber = "REF-99999999";

        var mockRepository = new Mock<IReservationRepository>();
        mockRepository.Setup(r => r.GetByReference(referenceNumber)).Returns((Reservation?)null);

        var mockProvider = new Mock<IHotelProvider>();
        var providers = new List<IHotelProvider> { mockProvider.Object };
        var service = new ReservationService(providers, mockRepository.Object);

        // Act & Assert
        var exception = Assert.Throws<ReservationNotFoundException>(() => service.GetByReference(referenceNumber));
        Assert.NotNull(exception.Message);
    }
}
