using System;
using HotelStay.Application.DTOs;
using HotelStay.Application.Mappers;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;
using Xunit;

namespace HotelStay.Tests.Application;

public class ReservationMapperTests
{
    private Room CreateTestRoom(decimal perNightRate = 1000m)
    {
        return Room.Create(
            roomId: Guid.NewGuid(),
            provider: "TestProvider",
            destination: "BOM",
            location: "India",
            roomType: RoomType.Deluxe,
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-05"),
            perNightRate: perNightRate,
            currency: "INR",
            cancellationPolicy: "FreeCancellation",
            amenities: new[] { "WiFi", "Pool" },
            starRating: 4
        );
    }

    [Fact]
    public void ToDocument_ShouldMapDtoToDocument()
    {
        // Arrange
        var dto = new DocumentDto
        {
            HolderName = "John Doe",
            Type = DocumentType.Passport,
            Number = "ABC123456"
        };

        // Act
        var document = ReservationMapper.ToDocument(dto);

        // Assert
        Assert.Equal("John Doe", document.HolderName);
        Assert.Equal(DocumentType.Passport, document.Type);
        Assert.Equal("ABC123456", document.Number);
    }

    [Fact]
    public void ToDocument_WithNationalId_ShouldMapCorrectly()
    {
        // Arrange
        var dto = new DocumentDto
        {
            HolderName = "Jane Smith",
            Type = DocumentType.NationalId,
            Number = "NAT987654"
        };

        // Act
        var document = ReservationMapper.ToDocument(dto);

        // Assert
        Assert.Equal(DocumentType.NationalId, document.Type);
        Assert.Equal("NAT987654", document.Number);
    }

    [Fact]
    public void ToResponse_ShouldMapReservationToResponse()
    {
        // Arrange
        var room = CreateTestRoom(perNightRate: 2000m);
        var document = Document.Create("John Doe", DocumentType.Passport, "ABC123456");
        var reservationTimestamp = DateTime.Parse("2024-03-15 10:30:45");
        
        var reservation = Reservation.Reserve(
            referenceNumber: "REF-12345678",
            roomId: room.RoomId,
            room: room,
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-05"),
            document: document,
            reservationTimestamp: reservationTimestamp
        );

        // Act
        var response = ReservationMapper.ToResponse(reservation);

        // Assert
        Assert.Equal("REF-12345678", response.ReferenceNumber);
        Assert.Equal(room.RoomId, response.RoomId);
        Assert.Equal("BOM", response.Destination);
        Assert.Equal("India", response.Location);
        Assert.Equal(RoomType.Deluxe, response.RoomType);
        Assert.Equal(DateTime.Parse("2024-04-01"), response.RoomCheckIn);
        Assert.Equal(DateTime.Parse("2024-04-05"), response.RoomCheckOut);
        Assert.Equal(DateTime.Parse("2024-04-01"), response.CheckIn);
        Assert.Equal(DateTime.Parse("2024-04-05"), response.CheckOut);
        Assert.Equal(4, response.NumberOfNights);
        Assert.Equal(8000m, response.TotalPrice); // 4 nights * 2000
        Assert.Equal("INR", response.Currency);
        Assert.Equal("TestProvider", response.Provider);
        Assert.Equal("2024-03-15 10:30:45", response.ReservationTimestamp);
    }

    [Fact]
    public void ToResponse_ShouldMapDocumentCorrectly()
    {
        // Arrange
        var room = CreateTestRoom();
        var document = Document.Create("Jane Smith", DocumentType.NationalId, "NAT987654");
        
        var reservation = Reservation.Reserve(
            referenceNumber: "REF-87654321",
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now
        );

        // Act
        var response = ReservationMapper.ToResponse(reservation);

        // Assert
        Assert.NotNull(response.Document);
        Assert.Equal("Jane Smith", response.Document.HolderName);
        Assert.Equal(DocumentType.NationalId, response.Document.Type);
        Assert.Equal("NAT987654", response.Document.Number);
    }

    [Fact]
    public void ToResponse_ShouldCalculateTotalPriceCorrectly()
    {
        // Arrange
        var room = CreateTestRoom(perNightRate: 1500m);
        var document = Document.Create("Test User", DocumentType.Passport, "TEST123");
        
        // Use dates within room's availability (room has checkIn: 2024-04-01, checkOut: 2024-04-05)
        var reservation = Reservation.Reserve(
            referenceNumber: "REF-TEST",
            roomId: room.RoomId,
            room: room,
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-04"), // 3 nights
            document: document,
            reservationTimestamp: DateTime.Now
        );

        // Act
        var response = ReservationMapper.ToResponse(reservation);

        // Assert
        Assert.Equal(3, response.NumberOfNights);
        Assert.Equal(4500m, response.TotalPrice); // 3 nights * 1500
    }

    [Fact]
    public void GenerateReferenceNumber_ShouldStartWithREF()
    {
        // Act
        var referenceNumber = ReservationMapper.GenerateReferenceNumber();

        // Assert
        Assert.StartsWith("REF-", referenceNumber);
    }

    [Fact]
    public void GenerateReferenceNumber_ShouldHaveCorrectLength()
    {
        // Act
        var referenceNumber = ReservationMapper.GenerateReferenceNumber();

        // Assert
        // Format: REF-12345678 (4 chars prefix + 8 chars guid)
        Assert.Equal(12, referenceNumber.Length);
    }

    [Fact]
    public void GenerateReferenceNumber_ShouldBeUnique()
    {
        // Act
        var ref1 = ReservationMapper.GenerateReferenceNumber();
        var ref2 = ReservationMapper.GenerateReferenceNumber();
        var ref3 = ReservationMapper.GenerateReferenceNumber();

        // Assert
        Assert.NotEqual(ref1, ref2);
        Assert.NotEqual(ref2, ref3);
        Assert.NotEqual(ref1, ref3);
    }

    [Fact]
    public void GenerateReferenceNumber_ShouldContainOnlyAlphanumeric()
    {
        // Act
        var referenceNumber = ReservationMapper.GenerateReferenceNumber();

        // Assert
        var guidPart = referenceNumber.Substring(4); // Skip "REF-"
        Assert.Matches("^[a-zA-Z0-9]+$", guidPart);
    }
}
