using System;
using HotelStay.Domain.Enums;
using HotelStay.Infrastructure.Providers.PremierStays;
using Xunit;

namespace HotelStay.Tests.Infrastructure;

public class PremierStaysMapperTests
{
    [Fact]
    public void ToRoom_ShouldMapAllProperties()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var checkIn = DateTime.Parse("2024-04-01");
        var checkOut = DateTime.Parse("2024-04-05");
        var amenities = new[] { "WiFi", "Pool", "Spa" };

        var entry = new PremierStaysRoomEntry
        {
            RoomId = roomId,
            Provider = "PremierStays",
            Destination = "BOM",
            Location = "India",
            RoomType = RoomType.Deluxe,
            CheckIn = checkIn,
            CheckOut = checkOut,
            PerNightRate = 5000m,
            Currency = "INR",
            CancellationPolicy = "FreeCancellation",
            Amenities = amenities,
            StarRating = 5
        };

        // Act
        var room = PremierStaysMapper.ToRoom(entry);

        // Assert
        Assert.Equal(roomId, room.RoomId);
        Assert.Equal("PremierStays", room.Provider);
        Assert.Equal("BOM", room.Destination);
        Assert.Equal("India", room.Location);
        Assert.Equal(RoomType.Deluxe, room.RoomType);
        Assert.Equal(checkIn, room.CheckIn);
        Assert.Equal(checkOut, room.CheckOut);
        Assert.Equal(5000m, room.PerNightRate);
        Assert.Equal("INR", room.Currency);
        Assert.Equal("FreeCancellation", room.CancellationPolicy);
        Assert.Equal(amenities, room.Amenities);
        Assert.Equal(5, room.StarRating);
    }

    [Fact]
    public void ToRoom_WithStandardRoom_ShouldMapCorrectly()
    {
        // Arrange
        var entry = new PremierStaysRoomEntry
        {
            RoomId = Guid.NewGuid(),
            Provider = "PremierStays",
            Destination = "LON",
            Location = "United Kingdom",
            RoomType = RoomType.Standard,
            CheckIn = DateTime.Parse("2024-05-10"),
            CheckOut = DateTime.Parse("2024-05-15"),
            PerNightRate = 3000m,
            Currency = "GBP",
            CancellationPolicy = "NonRefundable",
            Amenities = new[] { "WiFi" },
            StarRating = 3
        };

        // Act
        var room = PremierStaysMapper.ToRoom(entry);

        // Assert
        Assert.Equal(RoomType.Standard, room.RoomType);
        Assert.Equal("LON", room.Destination);
        Assert.Equal("GBP", room.Currency);
        Assert.Equal(3, room.StarRating);
    }

    [Fact]
    public void ToRoom_WithSuiteRoom_ShouldMapCorrectly()
    {
        // Arrange
        var entry = new PremierStaysRoomEntry
        {
            RoomId = Guid.NewGuid(),
            Provider = "PremierStays",
            Destination = "NYC",
            Location = "United States",
            RoomType = RoomType.Suite,
            CheckIn = DateTime.Parse("2024-06-01"),
            CheckOut = DateTime.Parse("2024-06-10"),
            PerNightRate = 8000m,
            Currency = "USD",
            CancellationPolicy = "FlexibleCancellation",
            Amenities = new[] { "WiFi", "Pool", "Spa", "Gym", "Restaurant" },
            StarRating = 5
        };

        // Act
        var room = PremierStaysMapper.ToRoom(entry);

        // Assert
        Assert.Equal(RoomType.Suite, room.RoomType);
        Assert.Equal("NYC", room.Destination);
        Assert.Equal(8000m, room.PerNightRate);
        Assert.Equal(5, room.Amenities.Length);
    }

    [Fact]
    public void ToRoom_WithNullStarRating_ShouldMapCorrectly()
    {
        // Arrange
        var entry = new PremierStaysRoomEntry
        {
            RoomId = Guid.NewGuid(),
            Provider = "PremierStays",
            Destination = "PAR",
            Location = "France",
            RoomType = RoomType.Deluxe,
            CheckIn = DateTime.Parse("2024-07-01"),
            CheckOut = DateTime.Parse("2024-07-05"),
            PerNightRate = 4000m,
            Currency = "EUR",
            CancellationPolicy = "FreeCancellation",
            Amenities = new[] { "WiFi", "Breakfast" },
            StarRating = null
        };

        // Act
        var room = PremierStaysMapper.ToRoom(entry);

        // Assert
        Assert.Null(room.StarRating);
    }

    [Fact]
    public void ToRoom_WithEmptyAmenities_ShouldMapCorrectly()
    {
        // Arrange
        var entry = new PremierStaysRoomEntry
        {
            RoomId = Guid.NewGuid(),
            Provider = "PremierStays",
            Destination = "TYO",
            Location = "Japan",
            RoomType = RoomType.Standard,
            CheckIn = DateTime.Parse("2024-08-01"),
            CheckOut = DateTime.Parse("2024-08-03"),
            PerNightRate = 2500m,
            Currency = "JPY",
            CancellationPolicy = "NonRefundable",
            Amenities = Array.Empty<string>(),
            StarRating = 2
        };

        // Act
        var room = PremierStaysMapper.ToRoom(entry);

        // Assert
        Assert.Empty(room.Amenities);
    }
}
