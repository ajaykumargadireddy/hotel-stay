using System;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;
using HotelStay.Domain.Exceptions;
using HotelStay.Domain.ValueObjects;
using Xunit;

namespace HotelStay.Tests.Domain;

public class ReservationTests
{
    private Room CreateTestRoom(string location = "India", DateTime? checkIn = null, DateTime? checkOut = null, decimal perNightRate = 1000m)
    {
        return Room.Create(
            roomId: Guid.NewGuid(),
            provider: "TestProvider",
            destination: "BOM",
            location: location,
            roomType: RoomType.Standard,
            checkIn: checkIn ?? DateTime.Parse("2024-04-01"),
            checkOut: checkOut ?? DateTime.Parse("2024-04-05"),
            perNightRate: perNightRate,
            currency: "INR",
            cancellationPolicy: "FreeCancellation",
            amenities: new[] { "WiFi" },
            starRating: 3
        );
    }

    private Document CreateTestDocument(DocumentType type = DocumentType.Passport, string number = "ABC123456")
    {
        return Document.Create("John Doe", type, number);
    }

    [Fact]
    public void Reserve_WithIndiaLocationAndNationalId_ShouldSucceed()
    {
        // Arrange
        var room = CreateTestRoom(location: "India");
        var document = CreateTestDocument(DocumentType.NationalId, "NAT123456");

        // Act
        var reservation = Reservation.Reserve(
            referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now);

        // Assert
        Assert.NotNull(reservation);
        Assert.NotNull(reservation.ReferenceNumber);
        Assert.Equal(room, reservation.Room);
        Assert.Equal(document, reservation.Document);
        Assert.True(reservation.ReservationTimestamp <= DateTime.Now);
    }

    [Fact]
    public void Reserve_WithIndiaLocationAndPassport_ShouldSucceed()
    {
        // Arrange
        var room = CreateTestRoom(location: "India");
        var document = CreateTestDocument(DocumentType.Passport, "P123456789");

        // Act
        var reservation = Reservation.Reserve(
            referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now);

        // Assert
        Assert.NotNull(reservation);
        Assert.NotNull(reservation.ReferenceNumber);
        Assert.Equal(room, reservation.Room);
        Assert.Equal(document, reservation.Document);
    }

    [Fact]
    public void Reserve_WithNonIndiaLocationAndPassport_ShouldSucceed()
    {
        // Arrange
        var room = CreateTestRoom(location: "United Kingdom");
        var document = CreateTestDocument(DocumentType.Passport, "P987654321");

        // Act
        var reservation = Reservation.Reserve(
            referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now);

        // Assert
        Assert.NotNull(reservation);
        Assert.NotNull(reservation.ReferenceNumber);
        Assert.Equal(room, reservation.Room);
        Assert.Equal(document, reservation.Document);
    }

    [Fact]
    public void Reserve_WithNonIndiaLocationAndNationalId_ShouldThrowDocumentMismatchException()
    {
        // Arrange
        var room = CreateTestRoom(location: "United States");
        var document = CreateTestDocument(DocumentType.NationalId, "NAT123456");

        // Act & Assert
        var exception = Assert.Throws<DocumentMismatchException>(() => Reservation.Reserve(
            referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now));
        Assert.Contains("international", exception.Message.ToLower());
        Assert.Contains("passport", exception.Message.ToLower());
    }

    [Fact]
    public void Reserve_WithCheckOutBeforeOrEqualToCheckIn_ShouldThrowDomainValidationException()
    {
        // Arrange - CheckOut equals CheckIn
        var room1 = CreateTestRoom(
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-01")
        );
        var document = CreateTestDocument();

        // Act & Assert
        var exception1 = Assert.Throws<DomainValidationException>(() => Reservation.Reserve(
            referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
            roomId: room1.RoomId,
            room: room1,
            checkIn: room1.CheckIn,
            checkOut: room1.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now));
        Assert.Contains("check-out", exception1.Message.ToLower());
        Assert.Contains("after", exception1.Message.ToLower());

        // Arrange - CheckOut before CheckIn
        var room2 = CreateTestRoom(
            checkIn: DateTime.Parse("2024-04-05"),
            checkOut: DateTime.Parse("2024-04-01")
        );

        // Act & Assert
        var exception2 = Assert.Throws<DomainValidationException>(() => Reservation.Reserve(
            referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
            roomId: room2.RoomId,
            room: room2,
            checkIn: room2.CheckIn,
            checkOut: room2.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now));
        Assert.Contains("check-out", exception2.Message.ToLower());
        Assert.Contains("after", exception2.Message.ToLower());
    }

    [Fact]
    public void Reserve_WithEmptyDocumentNumber_ShouldThrowDomainValidationException()
    {
        // Arrange
        var room = CreateTestRoom();

        // Act & Assert - empty string
        var exception1 = Assert.Throws<DomainValidationException>(() =>
        {
            var doc = Document.Create("John Doe", DocumentType.Passport, "");
            Reservation.Reserve(
                referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
                roomId: room.RoomId,
                room: room,
                checkIn: room.CheckIn,
                checkOut: room.CheckOut,
                document: doc,
                reservationTimestamp: DateTime.Now);
        });
        Assert.Contains("document", exception1.Message.ToLower());

        // Act & Assert - whitespace
        var exception2 = Assert.Throws<DomainValidationException>(() =>
        {
            var doc = Document.Create("John Doe", DocumentType.Passport, "   ");
            Reservation.Reserve(
                referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
                roomId: room.RoomId,
                room: room,
                checkIn: room.CheckIn,
                checkOut: room.CheckOut,
                document: doc,
                reservationTimestamp: DateTime.Now);
        });
        Assert.Contains("document", exception2.Message.ToLower());
    }

    [Fact]
    public void NumberOfNights_ShouldCalculateCorrectly()
    {
        // Arrange
        var room = CreateTestRoom(
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-05")
        );
        var document = CreateTestDocument();
        var reservation = Reservation.Reserve(
            referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now);

        // Act
        var numberOfNights = reservation.NumberOfNights;

        // Assert
        Assert.Equal(4, numberOfNights);
    }

    [Fact]
    public void TotalPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        var perNightRate = 1500m;
        var room = CreateTestRoom(
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-05"),
            perNightRate: perNightRate
        );
        var document = CreateTestDocument();
        var reservation = Reservation.Reserve(
            referenceNumber: "REF-" + Guid.NewGuid().ToString("N")[..8],
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now);

        // Act
        var totalPrice = reservation.TotalPrice;

        // Assert
        Assert.Equal(6000m, totalPrice); // 1500 * 4 nights
        Assert.Equal(perNightRate * reservation.NumberOfNights, totalPrice);
    }
}
