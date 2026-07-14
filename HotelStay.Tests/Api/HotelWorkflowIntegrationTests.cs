using System;
using System.Linq;
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

/// <summary>
/// Integration tests validating complete end-to-end workflows:
/// Search → Reserve → Lookup
/// Tests actual HTTP endpoints with in-memory state
/// </summary>
public class HotelWorkflowIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    // Use dates that match stub data (tomorrow + 5 days for India)
    private static readonly string CheckInDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
    private static readonly string CheckOutDate = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd");
    
    // UK dates (tomorrow + 3 days)
    private static readonly string UkCheckInDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
    private static readonly string UkCheckOutDate = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");

    public HotelWorkflowIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteWorkflow_SearchReserveLookup_DomesticWithNationalId_ShouldSucceed()
    {
        // Step 1: Search for hotels in Mumbai (domestic - India)
        var searchResponse = await _client.GetAsync(
            $"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}&roomType=Standard");
        
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>(JsonOptions);
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.NotEmpty(searchResult.Results);

        // Verify we got results from both providers
        var providers = searchResult.Results.Select(r => r.Room.Provider).Distinct().ToList();
        Assert.Contains("PremierStays", providers);
        Assert.Contains("BudgetNests", providers);

        // Step 2: Reserve a room with National ID (valid for domestic)
        var roomToReserve = searchResult.Results.First(r => r.Room.RoomType == RoomType.Standard);
        var reservationRequest = new ReservationRequest
        {
            RoomId = roomToReserve.Room.RoomId,
            CheckIn = DateTime.Parse(CheckInDate),
            CheckOut = DateTime.Parse(CheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "Raj Kumar",
                Type = DocumentType.NationalId,
                Number = "AADHAAR123456789"
            }
        };

        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reservationRequest, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, reserveResponse.StatusCode);
        
        var reservationResult = await reserveResponse.Content.ReadFromJsonAsync<ReservationCreatedResponse>(JsonOptions);
        Assert.NotNull(reservationResult);
        Assert.NotNull(reservationResult.ReferenceNumber);
        Assert.StartsWith("REF-", reservationResult.ReferenceNumber);

        // Step 3: Lookup the reservation
        var lookupResponse = await _client.GetAsync($"/hotels/reservation/{reservationResult.ReferenceNumber}");
        Assert.Equal(HttpStatusCode.OK, lookupResponse.StatusCode);
        
        var reservation = await lookupResponse.Content.ReadFromJsonAsync<ReservationResponse>(JsonOptions);
        Assert.NotNull(reservation);
        Assert.Equal(reservationResult.ReferenceNumber, reservation.ReferenceNumber);
        Assert.Equal(roomToReserve.Room.RoomId, reservation.RoomId);
        Assert.Equal("Raj Kumar", reservation.Document.HolderName);
        Assert.Equal(DocumentType.NationalId, reservation.Document.Type);
        Assert.Equal(roomToReserve.TotalPrice, reservation.TotalPrice);
        Assert.Equal(roomToReserve.TotalNights, reservation.NumberOfNights);
    }

    [Fact]
    public async Task CompleteWorkflow_SearchReserveLookup_InternationalWithPassport_ShouldSucceed()
    {
        // Step 1: Search for hotels in London (international - UK)
        var searchResponse = await _client.GetAsync(
            $"/hotels/search?destination=LON&checkIn={UkCheckInDate}&checkOut={UkCheckOutDate}&roomType=Deluxe");
        
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>(JsonOptions);
        Assert.NotNull(searchResult);
        Assert.NotNull(searchResult.Results);
        Assert.NotEmpty(searchResult.Results);

        // Step 2: Reserve a room with Passport (required for international)
        var roomToReserve = searchResult.Results.First();
        var reservationRequest = new ReservationRequest
        {
            RoomId = roomToReserve.Room.RoomId,
            CheckIn = DateTime.Parse(UkCheckInDate),
            CheckOut = DateTime.Parse(UkCheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "John Smith",
                Type = DocumentType.Passport,
                Number = "UK123456789"
            }
        };

        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reservationRequest, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, reserveResponse.StatusCode);
        
        var reservationResult = await reserveResponse.Content.ReadFromJsonAsync<ReservationCreatedResponse>(JsonOptions);
        Assert.NotNull(reservationResult);
        Assert.NotNull(reservationResult.ReferenceNumber);

        // Step 3: Lookup the reservation
        var lookupResponse = await _client.GetAsync($"/hotels/reservation/{reservationResult.ReferenceNumber}");
        Assert.Equal(HttpStatusCode.OK, lookupResponse.StatusCode);
        
        var reservation = await lookupResponse.Content.ReadFromJsonAsync<ReservationResponse>(JsonOptions);
        Assert.NotNull(reservation);
        Assert.Equal(reservationResult.ReferenceNumber, reservation.ReferenceNumber);
        Assert.Equal("John Smith", reservation.Document.HolderName);
        Assert.Equal(DocumentType.Passport, reservation.Document.Type);
    }

    [Fact]
    public async Task CompleteWorkflow_InternationalWithNationalId_ShouldReturn422()
    {
        // Step 1: Search for hotels in London (international)
        var searchResponse = await _client.GetAsync(
            $"/hotels/search?destination=LON&checkIn={UkCheckInDate}&checkOut={UkCheckOutDate}");
        
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>(JsonOptions);
        Assert.NotNull(searchResult);
        Assert.NotEmpty(searchResult.Results);

        // Step 2: Try to reserve with National ID (should fail - passport required for international)
        var roomToReserve = searchResult.Results.First();
        var reservationRequest = new ReservationRequest
        {
            RoomId = roomToReserve.Room.RoomId,
            CheckIn = DateTime.Parse(UkCheckInDate),
            CheckOut = DateTime.Parse(UkCheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "Jane Doe",
                Type = DocumentType.NationalId,
                Number = "INVALID123"
            }
        };

        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reservationRequest, JsonOptions);
        
        // Assert: Should return 422 Unprocessable Entity (document validation failed)
        Assert.Equal(HttpStatusCode.UnprocessableEntity, reserveResponse.StatusCode);
        
        var problemDetails = await reserveResponse.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Contains("Passport", problemDetails.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CompleteWorkflow_MultipleProvidersAggregation_ShouldReturnCombinedResults()
    {
        // Search Mumbai - both providers have data for BOM
        var searchResponse = await _client.GetAsync(
            $"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}");
        
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>(JsonOptions);
        Assert.NotNull(searchResult);
        Assert.NotEmpty(searchResult.Results);

        // Verify we have results from multiple providers
        var providerCounts = searchResult.Results
            .GroupBy(r => r.Room.Provider)
            .ToDictionary(g => g.Key, g => g.Count());

        Assert.True(providerCounts.Count >= 2, "Should have results from at least 2 providers");
        Assert.Contains("PremierStays", providerCounts.Keys);
        Assert.Contains("BudgetNests", providerCounts.Keys);

        // Verify each result has required data
        foreach (var result in searchResult.Results)
        {
            Assert.NotEqual(Guid.Empty, result.Room.RoomId);
            Assert.NotNull(result.Room.Provider);
            Assert.True(result.TotalNights > 0);
            Assert.True(result.TotalPrice > 0);
            Assert.Equal(result.Room.PerNightRate * result.TotalNights, result.TotalPrice);
        }
    }

    [Fact]
    public async Task CompleteWorkflow_ReservationLookupWithInvalidReference_ShouldReturn404()
    {
        // Try to lookup a reservation that doesn't exist
        var invalidReference = "REF-INVALID123";
        var lookupResponse = await _client.GetAsync($"/hotels/reservation/{invalidReference}");
        
        Assert.Equal(HttpStatusCode.NotFound, lookupResponse.StatusCode);
        
        var problemDetails = await lookupResponse.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Contains("not found", problemDetails.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CompleteWorkflow_ReserveWithInvalidRoomId_ShouldReturn400()
    {
        // Try to reserve a room that doesn't exist
        var reservationRequest = new ReservationRequest
        {
            RoomId = Guid.NewGuid(), // Non-existent room
            CheckIn = DateTime.Parse(CheckInDate),
            CheckOut = DateTime.Parse(CheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "Test User",
                Type = DocumentType.Passport,
                Number = "ABC123456"
            }
        };

        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reservationRequest, JsonOptions);
        
        Assert.Equal(HttpStatusCode.BadRequest, reserveResponse.StatusCode);
        
        var problemDetails = await reserveResponse.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.NotNull(problemDetails);
        Assert.Contains("Invalid roomId", problemDetails.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CompleteWorkflow_SearchWithRoomTypeFilter_ShouldReturnOnlyMatchingType()
    {
        // Search for only Deluxe rooms in Bangalore (BLR has Deluxe rooms in stub data)
        var searchResponse = await _client.GetAsync(
            $"/hotels/search?destination=BLR&checkIn={CheckInDate}&checkOut={CheckOutDate}&roomType=Deluxe");
        
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>(JsonOptions);
        Assert.NotNull(searchResult);
        Assert.NotEmpty(searchResult.Results);

        // Verify all results are Deluxe rooms
        Assert.All(searchResult.Results, r => Assert.Equal(RoomType.Deluxe, r.Room.RoomType));
    }

    [Fact]
    public async Task CompleteWorkflow_SearchWithoutRoomTypeFilter_ShouldReturnAllTypes()
    {
        // Search without room type filter - should return all types
        var searchResponse = await _client.GetAsync(
            $"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}");
        
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>(JsonOptions);
        Assert.NotNull(searchResult);
        Assert.NotEmpty(searchResult.Results);

        // Verify we have multiple room types
        var roomTypes = searchResult.Results.Select(r => r.Room.RoomType).Distinct().ToList();
        Assert.True(roomTypes.Count >= 2, "Should have results with different room types when no filter is applied");
    }

    [Fact]
    public async Task CompleteWorkflow_DomesticWithPassport_ShouldSucceed()
    {
        // Passport should be accepted for domestic destinations (India accepts both National ID and Passport)
        var searchResponse = await _client.GetAsync(
            $"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}");
        
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>(JsonOptions);
        Assert.NotNull(searchResult);
        Assert.NotEmpty(searchResult.Results);

        var roomToReserve = searchResult.Results.First();
        var reservationRequest = new ReservationRequest
        {
            RoomId = roomToReserve.Room.RoomId,
            CheckIn = DateTime.Parse(CheckInDate),
            CheckOut = DateTime.Parse(CheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "Priya Singh",
                Type = DocumentType.Passport,
                Number = "P987654321"
            }
        };

        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reservationRequest, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, reserveResponse.StatusCode);
        
        var reservationResult = await reserveResponse.Content.ReadFromJsonAsync<ReservationCreatedResponse>(JsonOptions);
        Assert.NotNull(reservationResult);
        Assert.NotNull(reservationResult.ReferenceNumber);
    }

    [Fact]
    public async Task CompleteWorkflow_ReservationPriceCalculation_ShouldMatchSearchResults()
    {
        // Search and reserve, then verify price calculation consistency
        var searchResponse = await _client.GetAsync(
            $"/hotels/search?destination=BOM&checkIn={CheckInDate}&checkOut={CheckOutDate}&roomType=Suite");
        
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<SearchResultWrapper>(JsonOptions);
        Assert.NotNull(searchResult);
        Assert.NotEmpty(searchResult.Results);

        var roomToReserve = searchResult.Results.First();
        var expectedTotalPrice = roomToReserve.TotalPrice;
        var expectedNights = roomToReserve.TotalNights;

        var reservationRequest = new ReservationRequest
        {
            RoomId = roomToReserve.Room.RoomId,
            CheckIn = DateTime.Parse(CheckInDate),
            CheckOut = DateTime.Parse(CheckOutDate),
            Document = new DocumentDto
            {
                HolderName = "Test Traveller",
                Type = DocumentType.Passport,
                Number = "TEST123456"
            }
        };

        var reserveResponse = await _client.PostAsJsonAsync("/hotels/reserve", reservationRequest, JsonOptions);
        var reservationResult = await reserveResponse.Content.ReadFromJsonAsync<ReservationCreatedResponse>(JsonOptions);
        Assert.NotNull(reservationResult);
        
        var lookupResponse = await _client.GetAsync($"/hotels/reservation/{reservationResult.ReferenceNumber}");
        var reservation = await lookupResponse.Content.ReadFromJsonAsync<ReservationResponse>(JsonOptions);
        Assert.NotNull(reservation);
        
        // Verify price calculations match
        Assert.Equal(expectedNights, reservation.NumberOfNights);
        Assert.Equal(expectedTotalPrice, reservation.TotalPrice);
        Assert.Equal(roomToReserve.Room.PerNightRate * expectedNights, reservation.TotalPrice);
    }

    // Helper classes for deserialization
    private class SearchResultWrapper
    {
        public HotelSearchResponse[] Results { get; set; } = Array.Empty<HotelSearchResponse>();
    }

    private class ProblemDetails
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
    }
}
