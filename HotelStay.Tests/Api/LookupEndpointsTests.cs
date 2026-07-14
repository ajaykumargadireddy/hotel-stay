using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HotelStay.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelStay.Tests.Api;

public class LookupEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LookupEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCountries_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/lookups/countries");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCountries_ShouldReturnAllCountries()
    {
        // Act
        var response = await _client.GetAsync("/lookups/countries");
        var countries = await response.Content.ReadFromJsonAsync<Country[]>();

        // Assert
        Assert.NotNull(countries);
        Assert.Equal(5, countries.Length);
        Assert.Contains(countries, c => c.Code == "IN" && c.Name == "India");
        Assert.Contains(countries, c => c.Code == "GB" && c.Name == "United Kingdom");
        Assert.Contains(countries, c => c.Code == "US" && c.Name == "United States");
        Assert.Contains(countries, c => c.Code == "JP" && c.Name == "Japan");
        Assert.Contains(countries, c => c.Code == "FR" && c.Name == "France");
    }

    [Fact]
    public async Task GetCities_WithoutCountryCode_ShouldReturnAllCities()
    {
        // Act
        var response = await _client.GetAsync("/lookups/cities");
        var cities = await response.Content.ReadFromJsonAsync<City[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cities);
        Assert.True(cities.Length >= 12);
    }

    [Fact]
    public async Task GetCities_WithValidCountryCode_ShouldReturnFilteredCities()
    {
        // Act
        var response = await _client.GetAsync("/lookups/cities?countryCode=IN");
        var cities = await response.Content.ReadFromJsonAsync<City[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cities);
        Assert.Equal(3, cities.Length);
        Assert.All(cities, c => Assert.Equal("IN", c.CountryCode));
        Assert.Contains(cities, c => c.Code == "BOM" && c.Name == "Mumbai");
        Assert.Contains(cities, c => c.Code == "DEL" && c.Name == "Delhi");
        Assert.Contains(cities, c => c.Code == "BLR" && c.Name == "Bangalore");
    }

    [Fact]
    public async Task GetCities_WithLowercaseCountryCode_ShouldReturnFilteredCities()
    {
        // Act
        var response = await _client.GetAsync("/lookups/cities?countryCode=gb");
        var cities = await response.Content.ReadFromJsonAsync<City[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cities);
        Assert.Equal(2, cities.Length);
        Assert.All(cities, c => Assert.Equal("GB", c.CountryCode));
    }

    [Fact]
    public async Task GetCities_WithInvalidCountryCode_ShouldReturnEmptyArray()
    {
        // Act
        var response = await _client.GetAsync("/lookups/cities?countryCode=ZZ");
        var cities = await response.Content.ReadFromJsonAsync<City[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cities);
        Assert.Empty(cities);
    }

    [Theory]
    [InlineData("IN", 3)]
    [InlineData("GB", 2)]
    [InlineData("US", 3)]
    [InlineData("JP", 2)]
    [InlineData("FR", 2)]
    public async Task GetCities_WithVariousCountries_ShouldReturnCorrectCount(string countryCode, int expectedCount)
    {
        // Act
        var response = await _client.GetAsync($"/lookups/cities?countryCode={countryCode}");
        var cities = await response.Content.ReadFromJsonAsync<City[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cities);
        Assert.Equal(expectedCount, cities.Length);
    }

    [Fact]
    public async Task GetCities_WithEmptyCountryCode_ShouldReturnAllCities()
    {
        // Act
        var response = await _client.GetAsync("/lookups/cities?countryCode=");
        var cities = await response.Content.ReadFromJsonAsync<City[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(cities);
        Assert.True(cities.Length >= 12);
    }
}
