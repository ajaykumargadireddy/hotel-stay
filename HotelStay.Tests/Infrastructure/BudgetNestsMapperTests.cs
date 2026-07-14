using System;
using HotelStay.Domain.Enums;
using HotelStay.Infrastructure.Providers.BudgetNests;
using Xunit;

namespace HotelStay.Tests.Infrastructure;

public class BudgetNestsMapperTests
{
    [Fact]
    public void ToRoom_ShouldMapAllProperties()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var checkIn = DateTime.Parse("2024-04-01");
        var checkOut = DateTime.Parse("2024-04-05");
        var amenities = new[] { "WiFi", "Parking" };

        var entry = new BudgetNestsRoomEntry
        {
            room_id = roomId,
            provider = "BudgetNests",
            destination = "BOM",
            location = "India",
            room_type = RoomType.Standard,
            check_in = checkIn,
            check_out = checkOut,
            per_night_rate = 1500m,
            currency = "INR",
            cancellation_policy = "FreeCancellation",
            amenities = amenities,
            star_rating = 2
        };

        // Act
        var room = BudgetNestsMapper.ToRoom(entry);

        // Assert
        Assert.Equal(roomId, room.RoomId);
        Assert.Equal("BudgetNests", room.Provider);
        Assert.Equal("BOM", room.Destination);
        Assert.Equal("India", room.Location);
        Assert.Equal(RoomType.Standard, room.RoomType);
        Assert.Equal(checkIn, room.CheckIn);
        Assert.Equal(checkOut, room.CheckOut);
        Assert.Equal(1500m, room.PerNightRate);
        Assert.Equal("INR", room.Currency);
        Assert.Equal("FreeCancellation", room.CancellationPolicy);
        Assert.Equal(amenities, room.Amenities);
        Assert.Equal(2, room.StarRating);
    }

    [Fact]
    public void ToRoom_WithDeluxeRoom_ShouldMapCorrectly()
    {
        // Arrange
        var entry = new BudgetNestsRoomEntry
        {
            room_id = Guid.NewGuid(),
            provider = "BudgetNests",
            destination = "DEL",
            location = "India",
            room_type = RoomType.Deluxe,
            check_in = DateTime.Parse("2024-05-10"),
            check_out = DateTime.Parse("2024-05-15"),
            per_night_rate = 2500m,
            currency = "INR",
            cancellation_policy = "NonRefundable",
            amenities = new[] { "WiFi", "Breakfast" },
            star_rating = 3
        };

        // Act
        var room = BudgetNestsMapper.ToRoom(entry);

        // Assert
        Assert.Equal(RoomType.Deluxe, room.RoomType);
        Assert.Equal("DEL", room.Destination);
        Assert.Equal(2500m, room.PerNightRate);
        Assert.Equal(3, room.StarRating);
    }

    [Fact]
    public void ToRoom_WithSuiteRoom_ShouldMapCorrectly()
    {
        // Arrange
        var entry = new BudgetNestsRoomEntry
        {
            room_id = Guid.NewGuid(),
            provider = "BudgetNests",
            destination = "LON",
            location = "United Kingdom",
            room_type = RoomType.Suite,
            check_in = DateTime.Parse("2024-06-01"),
            check_out = DateTime.Parse("2024-06-05"),
            per_night_rate = 4000m,
            currency = "GBP",
            cancellation_policy = "FlexibleCancellation",
            amenities = new[] { "WiFi", "Parking", "Gym" },
            star_rating = 4
        };

        // Act
        var room = BudgetNestsMapper.ToRoom(entry);

        // Assert
        Assert.Equal(RoomType.Suite, room.RoomType);
        Assert.Equal("LON", room.Destination);
        Assert.Equal("GBP", room.Currency);
        Assert.Equal(4, room.StarRating);
    }

    [Fact]
    public void ToRoom_WithNullStarRating_ShouldMapCorrectly()
    {
        // Arrange
        var entry = new BudgetNestsRoomEntry
        {
            room_id = Guid.NewGuid(),
            provider = "BudgetNests",
            destination = "NYC",
            location = "United States",
            room_type = RoomType.Standard,
            check_in = DateTime.Parse("2024-07-01"),
            check_out = DateTime.Parse("2024-07-05"),
            per_night_rate = 3000m,
            currency = "USD",
            cancellation_policy = "NonRefundable",
            amenities = new[] { "WiFi" },
            star_rating = null
        };

        // Act
        var room = BudgetNestsMapper.ToRoom(entry);

        // Assert
        Assert.Null(room.StarRating);
    }

    [Fact]
    public void ToRoom_WithEmptyAmenities_ShouldMapCorrectly()
    {
        // Arrange
        var entry = new BudgetNestsRoomEntry
        {
            room_id = Guid.NewGuid(),
            provider = "BudgetNests",
            destination = "PAR",
            location = "France",
            room_type = RoomType.Standard,
            check_in = DateTime.Parse("2024-08-01"),
            check_out = DateTime.Parse("2024-08-03"),
            per_night_rate = 1800m,
            currency = "EUR",
            cancellation_policy = "FreeCancellation",
            amenities = Array.Empty<string>(),
            star_rating = 2
        };

        // Act
        var room = BudgetNestsMapper.ToRoom(entry);

        // Assert
        Assert.Empty(room.Amenities);
    }

    [Fact]
    public void ToRoom_WithNullAmenities_ShouldMapToEmptyArray()
    {
        // Arrange
        var entry = new BudgetNestsRoomEntry
        {
            room_id = Guid.NewGuid(),
            provider = "BudgetNests",
            destination = "TYO",
            location = "Japan",
            room_type = RoomType.Deluxe,
            check_in = DateTime.Parse("2024-09-01"),
            check_out = DateTime.Parse("2024-09-05"),
            per_night_rate = 3500m,
            currency = "JPY",
            cancellation_policy = "FlexibleCancellation",
            amenities = null!,
            star_rating = 3
        };

        // Act
        var room = BudgetNestsMapper.ToRoom(entry);

        // Assert
        Assert.NotNull(room.Amenities);
        Assert.Empty(room.Amenities);
    }
}
