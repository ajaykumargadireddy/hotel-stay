using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public async Task Reserve_ShouldCallGetRoomByIdOnProvidersUntilFound()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = CreateTestRoom(roomId);

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync((Room?)null);

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var mockProvider3 = new Mock<IHotelProvider>();
        mockProvider3.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync((Room?)null);

        var mockRepository = new Mock<IReservationRepository>();
        mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

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
        var response = await service.ReserveAsync(request);

        // Assert
        Assert.NotNull(response);
        mockProvider1.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
        mockProvider2.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
        mockProvider3.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reserve_WhenRoomIdNotFoundInAnyProvider_ShouldThrowDomainValidationException()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync((Room?)null);

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync((Room?)null);

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
        var exception = await Assert.ThrowsAsync<DomainValidationException>(() => service.ReserveAsync(request));
    }

    [Fact]
    public async Task Reserve_ShouldGenerateReferenceNumberWithCorrectFormat()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = CreateTestRoom(roomId);

        var mockProvider = new Mock<IHotelProvider>();
        mockProvider.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var mockRepository = new Mock<IReservationRepository>();
        mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

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
        var response = await service.ReserveAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.ReferenceNumber);
        Assert.StartsWith("REF-", response.ReferenceNumber);
        Assert.Equal(12, response.ReferenceNumber.Length); // REF- + 8 hex chars
        Assert.Matches(@"^REF-[a-f0-9]{8}$", response.ReferenceNumber);
    }

    [Fact]
    public async Task Reserve_ShouldPersistReservationViaRepository()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = CreateTestRoom(roomId);

        var mockProvider = new Mock<IHotelProvider>();
        mockProvider.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var mockRepository = new Mock<IReservationRepository>();
        Reservation? capturedReservation = null;
        mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Callback<Reservation, CancellationToken>((r, ct) => capturedReservation = r)
            .Returns(Task.CompletedTask);

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
        var response = await service.ReserveAsync(request);

        // Assert
        mockRepository.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(capturedReservation);
        Assert.Equal(room, capturedReservation.Room);
        Assert.Equal("Jane Smith", capturedReservation.Document.HolderName);
        Assert.Equal(DocumentType.Passport, capturedReservation.Document.Type);
        Assert.Equal("P987654321", capturedReservation.Document.Number);
    }

    [Fact]
    public async Task GetByReference_ShouldRetrieveReservationSuccessfully()
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
        mockRepository.Setup(r => r.GetByReferenceAsync(referenceNumber, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        var mockProvider = new Mock<IHotelProvider>();
        var providers = new List<IHotelProvider> { mockProvider.Object };
        var service = new ReservationService(providers, mockRepository.Object);

        // Act
        var response = await service.GetByReferenceAsync(referenceNumber);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(reservation.ReferenceNumber, response.ReferenceNumber);
        Assert.Equal(room.RoomId, response.RoomId);
        Assert.Equal("John Doe", response.Document.HolderName);
        mockRepository.Verify(r => r.GetByReferenceAsync(referenceNumber, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByReference_WhenNotFound_ShouldThrowReservationNotFoundException()
    {
        // Arrange
        var referenceNumber = "REF-99999999";

        var mockRepository = new Mock<IReservationRepository>();
        mockRepository.Setup(r => r.GetByReferenceAsync(referenceNumber, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation?)null);

        var mockProvider = new Mock<IHotelProvider>();
        var providers = new List<IHotelProvider> { mockProvider.Object };
        var service = new ReservationService(providers, mockRepository.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ReservationNotFoundException>(() => service.GetByReferenceAsync(referenceNumber));
        Assert.NotNull(exception.Message);
    }
}
