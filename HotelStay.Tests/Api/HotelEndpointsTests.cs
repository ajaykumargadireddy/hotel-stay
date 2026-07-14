using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HotelStay.Application.DTOs;
using HotelStay.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelStay.Tests.Api;

public class HotelEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    // Use dates that match stub data (tomorrow + 5 days for BOM)
    private static readonly string CheckInDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
    private static readonly string CheckOutDate = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd");

    public HotelEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchEndpoint_WithValidParameters_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}&roomType=Deluxe");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SearchResultWrapper>();
        Assert.NotNull(result);
        Assert.NotNull(result.Results);
    }

    [Fact]
    public async Task SearchEndpoint_WithoutDestination_ShouldReturn400()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?checkIn={CheckInDate}&checkOut={CheckOutDate}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchEndpoint_WithoutCheckIn_ShouldReturn400()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?destination=BOM&checkOut={CheckOutDate}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchEndpoint_WithoutCheckOut_ShouldReturn400()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckInDate}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchEndpoint_WithCheckOutBeforeCheckIn_ShouldReturn400()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckOutDate}&checkOut={CheckInDate}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchEndpoint_WithCheckOutEqualCheckIn_ShouldReturn400()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckInDate}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchEndpoint_WithoutRoomType_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("Standard")]
    [InlineData("Deluxe")]
    [InlineData("Suite")]
    public async Task SearchEndpoint_WithDifferentRoomTypes_ShouldReturn200(string roomType)
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}&roomType={roomType}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReserveEndpoint_WithValidRequest_ShouldReturn201()
    {
        // Arrange
        var searchResponse = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}");
        searchResponse.EnsureSuccessStatusCode();
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>();
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.NotEmpty(searchResult.Results);
        var roomId = searchResult.Results[0].Room.RoomId;

        var request = new ReservationRequest
        {
            RoomId = roomId,
            CheckIn = DateTime.Parse(CheckInDate),
            CheckOut = DateTime.Parse(CheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "John Doe",
                Type = DocumentType.Passport,
                Number = "ABC123456"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReservationCreatedResponse>();
        Assert.NotNull(result);
        Assert.StartsWith("REF-", result.ReferenceNumber);
    }

    [Fact]
    public async Task ReserveEndpoint_WithInvalidRoomId_ShouldReturn400()
    {
        // Arrange
        var request = new ReservationRequest
        {
            RoomId = Guid.NewGuid(),
            CheckIn = DateTime.Parse(CheckInDate),
            CheckOut = DateTime.Parse(CheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "John Doe",
                Type = DocumentType.Passport,
                Number = "ABC123456"
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/hotels/reserve", request);

        // Assert - Returns 400 because invalid roomId is a validation error
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetRoomById_WithValidId_ShouldReturn200()
    {
        // Arrange - First search to get a valid room ID
        var searchResponse = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}");
        searchResponse.EnsureSuccessStatusCode();
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>();
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.NotEmpty(searchResult.Results);
        var roomId = searchResult.Results[0].Room.RoomId;

        // Act
        var response = await _client.GetAsync($"/hotels/room/{roomId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RoomDetailsResponse>();
        Assert.NotNull(result);
        Assert.Equal(roomId, result.RoomId);
    }

    [Fact]
    public async Task GetRoomById_WithInvalidId_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/room/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReservation_WithValidReference_ShouldReturn200()
    {
        // Arrange - First create a reservation
        var searchResponse = await _client.GetAsync($"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}");
        searchResponse.EnsureSuccessStatusCode();
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>();
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.NotEmpty(searchResult.Results);
        var roomId = searchResult.Results[0].Room.RoomId;

        var reserveRequest = new ReservationRequest
        {
            RoomId = roomId,
            CheckIn = DateTime.Parse(CheckInDate),
            CheckOut = DateTime.Parse(CheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "Jane Smith",
                Type = DocumentType.Passport,
                Number = "XYZ987654"
            }
        };

        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reserveRequest);
        var reserveResult = await reserveResponse.Content.ReadFromJsonAsync<ReservationCreatedResponse>();
        var reference = reserveResult?.ReferenceNumber ?? "REF-TEST";

        // Act
        var response = await _client.GetAsync($"/hotels/reservation/{reference}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReservationResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(reference, result.ReferenceNumber);
        Assert.Equal("Jane Smith", result.Document.HolderName);
    }

    [Fact]
    public async Task GetReservation_WithInvalidReference_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/hotels/reservation/REF-INVALID");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchEndpoint_ResponseShouldContainCorrectStructure()
    {
        // Act
        var response = await _client.GetAsync($"/hotels/search?destination=NYC&checkIn={CheckInDate}&checkOut={CheckOutDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SearchResultWrapper>();
        Assert.NotNull(result);
        Assert.NotNull(result.Results);
        
        if (result.Results.Length > 0)
        {
            var firstResult = result.Results[0];
            Assert.NotNull(firstResult.Room);
            Assert.NotEqual(Guid.Empty, firstResult.Room.RoomId);
            Assert.NotEmpty(firstResult.Room.Provider);
            Assert.True(firstResult.TotalNights > 0);
            Assert.True(firstResult.TotalPrice > 0);
        }
    }

    // Helper class for deserializing search response
    private class SearchResultWrapper
    {
        public HotelSearchResponse[] Results { get; set; } = Array.Empty<HotelSearchResponse>();
    }
}
