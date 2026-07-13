using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;
using HotelStay.Application.Services;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;
using Moq;
using Xunit;

namespace HotelStay.Tests.Application;

public class HotelSearchServiceTests
{
    private Room CreateTestRoom(string provider, Guid roomId, string destination = "BOM", RoomType roomType = RoomType.Standard)
    {
        return Room.Create(
            roomId: roomId,
            provider: provider,
            destination: destination,
            location: "India",
            roomType: roomType,
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
    public async Task Search_ShouldAggregateResultsFromMultipleProviders()
    {
        // Arrange
        var request = new HotelSearchRequest
        {
            Destination = "BOM",
            CheckIn = DateTime.Parse("2024-04-01"),
            CheckOut = DateTime.Parse("2024-04-05"),
            RoomType = RoomType.Standard
        };

        var provider1Room = CreateTestRoom("Provider1", Guid.NewGuid());
        var provider2Room = CreateTestRoom("Provider2", Guid.NewGuid());

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.SearchAsync(It.IsAny<HotelSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { provider1Room });

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.SearchAsync(It.IsAny<HotelSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { provider2Room });

        var providers = new List<IHotelProvider> { mockProvider1.Object, mockProvider2.Object };
        var service = new HotelSearchService(providers);

        // Act
        var results = (await service.SearchAsync(request)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Room.Provider == "Provider1");
        Assert.Contains(results, r => r.Room.Provider == "Provider2");
        mockProvider1.Verify(p => p.SearchAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        mockProvider2.Verify(p => p.SearchAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WhenNoProvidersHaveMatchingRooms_ShouldReturnEmptyList()
    {
        // Arrange
        var request = new HotelSearchRequest
        {
            Destination = "BOM",
            CheckIn = DateTime.Parse("2024-04-01"),
            CheckOut = DateTime.Parse("2024-04-05"),
            RoomType = RoomType.Deluxe
        };

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.SearchAsync(It.IsAny<HotelSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Room>());

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.SearchAsync(It.IsAny<HotelSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Room>());

        var providers = new List<IHotelProvider> { mockProvider1.Object, mockProvider2.Object };
        var service = new HotelSearchService(providers);

        // Act
        var results = (await service.SearchAsync(request)).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task Search_RoomComputedProperties_ShouldCalculateCorrectly()
    {
        // Arrange
        var request = new HotelSearchRequest
        {
            Destination = "BOM",
            CheckIn = DateTime.Parse("2024-04-01"),
            CheckOut = DateTime.Parse("2024-04-05")
        };

        var room = Room.Create(
            roomId: Guid.NewGuid(),
            provider: "TestProvider",
            destination: "BOM",
            location: "India",
            roomType: RoomType.Standard,
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-05"),
            perNightRate: 2500m,
            currency: "INR",
            cancellationPolicy: "FreeCancellation",
            amenities: new[] { "WiFi" },
            starRating: 3
        );

        var mockProvider = new Mock<IHotelProvider>();
        mockProvider.Setup(p => p.SearchAsync(It.IsAny<HotelSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { room });

        var providers = new List<IHotelProvider> { mockProvider.Object };
        var service = new HotelSearchService(providers);

        // Act
        var results = (await service.SearchAsync(request)).ToList();

        // Assert
        Assert.Single(results);
        var result = results[0];
        Assert.Equal(4, result.TotalNights); // Apr 1 to Apr 5 = 4 nights
        Assert.Equal(10000m, result.TotalPrice); // 2500 * 4 = 10000
    }

    [Fact]
    public async Task GetRoomByIdAsync_WhenRoomFoundInProvider_ShouldReturnRoomDetails()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = Room.Create(
            roomId: roomId,
            provider: "TestProvider",
            destination: "BOM",
            location: "India",
            roomType: RoomType.Deluxe,
            checkIn: DateTime.Parse("2024-04-01"),
            checkOut: DateTime.Parse("2024-04-05"),
            perNightRate: 5000m,
            currency: "INR",
            cancellationPolicy: "FreeCancellation",
            amenities: new[] { "WiFi", "Pool" },
            starRating: 4
        );

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var providers = new List<IHotelProvider> { mockProvider1.Object, mockProvider2.Object };
        var service = new HotelSearchService(providers);

        // Act
        var result = await service.GetRoomByIdAsync(roomId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roomId, result.RoomId);
        Assert.Equal("TestProvider", result.Provider);
        Assert.Equal("BOM", result.Destination);
        Assert.Equal("India", result.Location);
        Assert.Equal("Deluxe", result.RoomType);
        Assert.Equal(5000m, result.PerNightRate);
        Assert.Equal("INR", result.Currency);
        Assert.Equal("FreeCancellation", result.CancellationPolicy);
        Assert.Equal(2, result.Amenities.Length);
        Assert.Contains("WiFi", result.Amenities);
        Assert.Contains("Pool", result.Amenities);
        Assert.Equal(4, result.StarRating);
        
        mockProvider1.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
        mockProvider2.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRoomByIdAsync_WhenRoomNotFoundInAnyProvider_ShouldReturnNull()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var providers = new List<IHotelProvider> { mockProvider1.Object, mockProvider2.Object };
        var service = new HotelSearchService(providers);

        // Act
        var result = await service.GetRoomByIdAsync(roomId);

        // Assert
        Assert.Null(result);
        mockProvider1.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
        mockProvider2.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRoomByIdAsync_ShouldQueryAllProvidersInParallel()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = CreateTestRoom("Provider3", roomId);

        var mockProvider1 = new Mock<IHotelProvider>();
        mockProvider1.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var mockProvider2 = new Mock<IHotelProvider>();
        mockProvider2.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Room?)null);

        var mockProvider3 = new Mock<IHotelProvider>();
        mockProvider3.Setup(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var providers = new List<IHotelProvider> { mockProvider1.Object, mockProvider2.Object, mockProvider3.Object };
        var service = new HotelSearchService(providers);

        // Act
        var result = await service.GetRoomByIdAsync(roomId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Provider3", result.Provider);
        
        // Verify all providers were queried
        mockProvider1.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
        mockProvider2.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
        mockProvider3.Verify(p => p.GetRoomByIdAsync(roomId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
