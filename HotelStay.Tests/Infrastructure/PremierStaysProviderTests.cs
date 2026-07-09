using System;
using System.Linq;
using HotelStay.Application.DTOs;
using HotelStay.Domain.Enums;
using HotelStay.Infrastructure.Providers;
using HotelStay.Infrastructure.Providers.PremierStays;
using Xunit;

namespace HotelStay.Tests.Infrastructure;

public class PremierStaysProviderTests
{
    [Fact]
    public void Search_ShouldFilterByDestination_CaseInsensitive()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var request = new HotelSearchRequest
        {
            Destination = "bom", // lowercase
            CheckIn = StubDates.IndiaCheckIn,
            CheckOut = StubDates.IndiaCheckOut
        };

        // Act
        var results = provider.Search(request).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal("BOM", r.Destination, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void Search_ShouldFilterByRoomType()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var request = new HotelSearchRequest
        {
            Destination = "BOM",
            CheckIn = StubDates.IndiaCheckIn,
            CheckOut = StubDates.IndiaCheckOut.AddDays(-1),
            RoomType = RoomType.Standard
        };

        // Act
        var results = provider.Search(request).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(RoomType.Standard, r.RoomType));
    }

    [Fact]
    public void Search_ShouldFilterByDateRange()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var request = new HotelSearchRequest
        {
            Destination = "BOM",
            CheckIn = StubDates.IndiaCheckIn.AddDays(1),  // Within stub range
            CheckOut = StubDates.IndiaCheckOut.AddDays(-1)
        };

        // Act
        var results = provider.Search(request).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.All(results, r =>
        {
            Assert.True(request.CheckIn >= r.CheckIn, "CheckIn should be >= room CheckIn");
            Assert.True(request.CheckOut <= r.CheckOut, "CheckOut should be <= room CheckOut");
        });
    }

    [Fact]
    public void Search_ShouldReturnDeterministicResults()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var request = new HotelSearchRequest
        {
            Destination = "BOM",
            CheckIn = StubDates.IndiaCheckIn,
            CheckOut = StubDates.IndiaCheckOut,
            RoomType = RoomType.Standard
        };

        // Act
        var results1 = provider.Search(request).ToList();
        var results2 = provider.Search(request).ToList();

        // Assert
        Assert.Equal(results1.Count, results2.Count);
        for (int i = 0; i < results1.Count; i++)
        {
            Assert.Equal(results1[i].RoomId, results2[i].RoomId);
            Assert.Equal(results1[i].Provider, results2[i].Provider);
            Assert.Equal(results1[i].PerNightRate, results2[i].PerNightRate);
        }
    }

    [Fact]
    public void GetRoomById_WhenGuidExists_ShouldReturnRoom()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var roomId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Known room ID from stub data

        // Act
        var room = provider.GetRoomById(roomId);

        // Assert
        Assert.NotNull(room);
        Assert.Equal(roomId, room.RoomId);
        Assert.Equal("PremierStays", room.Provider);
    }

    [Fact]
    public void GetRoomById_WhenGuidNotFound_ShouldReturnNull()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var nonExistentRoomId = Guid.NewGuid();

        // Act
        var room = provider.GetRoomById(nonExistentRoomId);

        // Assert
        Assert.Null(room);
    }

    [Fact]
    public void Search_WithNonExistentDestination_ShouldReturnEmpty()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var request = new HotelSearchRequest
        {
            Destination = "INVALID",
            CheckIn = StubDates.IndiaCheckIn,
            CheckOut = StubDates.IndiaCheckOut
        };

        // Act
        var results = provider.Search(request).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Search_WithDateOutsideAvailableRange_ShouldReturnEmpty()
    {
        // Arrange
        var provider = new PremierStaysProvider();
        var request = new HotelSearchRequest
        {
            Destination = "BOM",
            CheckIn = StubDates.IndiaCheckIn.AddYears(5), // Far outside stub range
            CheckOut = StubDates.IndiaCheckOut.AddYears(5)
        };

        // Act
        var results = provider.Search(request).ToList();

        // Assert
        Assert.Empty(results);
    }
}
