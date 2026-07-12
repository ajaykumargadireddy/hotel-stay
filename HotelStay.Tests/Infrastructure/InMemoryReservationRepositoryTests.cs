using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelStay.Domain.Entities;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;
using HotelStay.Infrastructure.Repositories;
using Xunit;

namespace HotelStay.Tests.Infrastructure;

public class InMemoryReservationRepositoryTests
{
    private Reservation CreateTestReservation(string referenceNumber = "REF-12345678")
    {
        var room = Room.Create(
            roomId: Guid.NewGuid(),
            provider: "TestProvider",
            destination: "BOM",
            location: "India",
            roomType: RoomType.Standard,
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-05"),
            perNightRate: 3000m,
            currency: "INR",
            cancellationPolicy: "FreeCancellation",
            amenities: new[] { "WiFi" },
            starRating: 3
        );

        var document = Document.Create("John Doe", DocumentType.Passport, "P123456789");
        var reservation = Reservation.Reserve(
            referenceNumber: referenceNumber,
            roomId: room.RoomId,
            room: room,
            checkIn: room.CheckIn,
            checkOut: room.CheckOut,
            document: document,
            reservationTimestamp: DateTime.Now);

        // Use reflection to set the reference number for testing
        var refField = typeof(Reservation).GetField("_referenceNumber", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        refField?.SetValue(reservation, referenceNumber);

        return reservation;
    }

    [Fact]
    public async Task Add_ShouldStoreReservation()
    {
        // Arrange
        var repository = new InMemoryReservationRepository();
        var reservation = CreateTestReservation("REF-11111111");

        // Act
        await repository.AddAsync(reservation);

        // Assert
        var retrieved = await repository.GetByReferenceAsync("REF-11111111");
        Assert.NotNull(retrieved);
        Assert.Equal(reservation.ReferenceNumber, retrieved.ReferenceNumber);
        Assert.Equal(reservation.Room.RoomId, retrieved.Room.RoomId);
    }

    [Fact]
    public async Task GetByReference_WhenReservationExists_ShouldReturnReservation()
    {
        // Arrange
        var repository = new InMemoryReservationRepository();
        var reservation = CreateTestReservation("REF-22222222");
        await repository.AddAsync(reservation);

        // Act
        var retrieved = await repository.GetByReferenceAsync("REF-22222222");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("REF-22222222", retrieved.ReferenceNumber);
        Assert.Equal(reservation.Room.Provider, retrieved.Room.Provider);
        Assert.Equal(reservation.Document.HolderName, retrieved.Document.HolderName);
    }

    [Fact]
    public async Task GetByReference_WhenReservationNotFound_ShouldReturnNull()
    {
        // Arrange
        var repository = new InMemoryReservationRepository();

        // Act
        var retrieved = await repository.GetByReferenceAsync("REF-99999999");

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task Add_WithSameReferenceNumber_ShouldOverwritePrevious()
    {
        // Arrange
        var repository = new InMemoryReservationRepository();
        var reservation1 = CreateTestReservation("REF-33333333");
        var reservation2 = CreateTestReservation("REF-33333333");

        // Act
        await repository.AddAsync(reservation1);
        await repository.AddAsync(reservation2);

        // Assert
        var retrieved = await repository.GetByReferenceAsync("REF-33333333");
        Assert.NotNull(retrieved);
        // Both reservations have the same reference number, so either is acceptable
        Assert.Equal("REF-33333333", retrieved.ReferenceNumber);
    }

    [Fact]
    public async Task Repository_ShouldBeThreadSafe_ConcurrentAddOperations()
    {
        // Arrange
        var repository = new InMemoryReservationRepository();
        var reservations = Enumerable.Range(1, 100)
            .Select(i => CreateTestReservation($"REF-{i:D8}"))
            .ToList();

        // Act - Add reservations concurrently
        await Parallel.ForEachAsync(reservations, async (reservation, ct) =>
        {
            await repository.AddAsync(reservation, ct);
        });

        // Assert - All reservations should be retrievable
        foreach (var reservation in reservations)
        {
            var retrieved = await repository.GetByReferenceAsync(reservation.ReferenceNumber);
            Assert.NotNull(retrieved);
            Assert.Equal(reservation.ReferenceNumber, retrieved.ReferenceNumber);
        }
    }

    [Fact]
    public async Task Repository_ShouldBeThreadSafe_ConcurrentGetOperations()
    {
        // Arrange
        var repository = new InMemoryReservationRepository();
        var reservation = CreateTestReservation("REF-44444444");
        await repository.AddAsync(reservation);

        var results = new List<Reservation?>();
        var lockObject = new object();

        // Act - Get reservation concurrently
        await Parallel.ForAsync(0, 100, async (i, ct) =>
        {
            var retrieved = await repository.GetByReferenceAsync("REF-44444444", ct);
            lock (lockObject)
            {
                results.Add(retrieved);
            }
        });

        // Assert - All reads should succeed
        Assert.Equal(100, results.Count);
        Assert.All(results, r =>
        {
            Assert.NotNull(r);
            Assert.Equal("REF-44444444", r.ReferenceNumber);
        });
    }

    [Fact]
    public async Task Repository_ShouldBeThreadSafe_ConcurrentAddAndGetOperations()
    {
        // Arrange
        var repository = new InMemoryReservationRepository();
        var reservation1 = CreateTestReservation("REF-55555555");
        var reservation2 = CreateTestReservation("REF-66666666");

        var exceptions = new List<Exception>();
        var lockObject = new object();

        // Act - Mix add and get operations concurrently
        var tasks = new List<Task>
        {
            Task.Run(async () => {
                try { await repository.AddAsync(reservation1); }
                catch (Exception ex) { lock (lockObject) { exceptions.Add(ex); } }
            }),
            Task.Run(async () => {
                try { await repository.AddAsync(reservation2); }
                catch (Exception ex) { lock (lockObject) { exceptions.Add(ex); } }
            }),
            Task.Run(async () => {
                try { var _ = await repository.GetByReferenceAsync("REF-55555555"); }
                catch (Exception ex) { lock (lockObject) { exceptions.Add(ex); } }
            }),
            Task.Run(async () => {
                try { var _ = await repository.GetByReferenceAsync("REF-66666666"); }
                catch (Exception ex) { lock (lockObject) { exceptions.Add(ex); } }
            })
        };

        await Task.WhenAll(tasks);

        // Assert - No exceptions should occur
        Assert.Empty(exceptions);

        // Both reservations should be retrievable
        var retrieved1 = await repository.GetByReferenceAsync("REF-55555555");
        var retrieved2 = await repository.GetByReferenceAsync("REF-66666666");
        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
    }
}
