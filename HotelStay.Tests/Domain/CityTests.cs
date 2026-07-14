using HotelStay.Domain.Entities;
using Xunit;

namespace HotelStay.Tests.Domain;

public class CityTests
{
    [Fact]
    public void All_ShouldContainPredefinedCities()
    {
        // Act
        var cities = City.All;

        // Assert
        Assert.NotEmpty(cities);
        Assert.Contains(cities, c => c.Code == "BOM" && c.Name == "Mumbai" && c.CountryCode == "IN");
        Assert.Contains(cities, c => c.Code == "DEL" && c.Name == "Delhi" && c.CountryCode == "IN");
        Assert.Contains(cities, c => c.Code == "LON" && c.Name == "London" && c.CountryCode == "GB");
        Assert.Contains(cities, c => c.Code == "NYC" && c.Name == "New York" && c.CountryCode == "US");
    }

    [Fact]
    public void GetByCode_WithValidCode_ShouldReturnCity()
    {
        // Act
        var city = City.GetByCode("BOM");

        // Assert
        Assert.NotNull(city);
        Assert.Equal("BOM", city.Code);
        Assert.Equal("Mumbai", city.Name);
        Assert.Equal("IN", city.CountryCode);
    }

    [Fact]
    public void GetByCode_WithLowercaseCode_ShouldReturnCity()
    {
        // Act
        var city = City.GetByCode("bom");

        // Assert
        Assert.NotNull(city);
        Assert.Equal("BOM", city.Code);
    }

    [Fact]
    public void GetByCode_WithInvalidCode_ShouldReturnNull()
    {
        // Act
        var city = City.GetByCode("XYZ");

        // Assert
        Assert.Null(city);
    }

    [Fact]
    public void GetByCode_WithEmptyString_ShouldReturnNull()
    {
        // Act
        var city = City.GetByCode("");

        // Assert
        Assert.Null(city);
    }

    [Fact]
    public void GetByCountryCode_WithValidCountryCode_ShouldReturnCities()
    {
        // Act
        var cities = City.GetByCountryCode("IN");

        // Assert
        Assert.NotEmpty(cities);
        Assert.Equal(3, cities.Count);
        Assert.All(cities, c => Assert.Equal("IN", c.CountryCode));
        Assert.Contains(cities, c => c.Code == "BOM");
        Assert.Contains(cities, c => c.Code == "DEL");
        Assert.Contains(cities, c => c.Code == "BLR");
    }

    [Fact]
    public void GetByCountryCode_WithLowercaseCountryCode_ShouldReturnCities()
    {
        // Act
        var cities = City.GetByCountryCode("gb");

        // Assert
        Assert.NotEmpty(cities);
        Assert.Equal(2, cities.Count);
        Assert.All(cities, c => Assert.Equal("GB", c.CountryCode));
    }

    [Fact]
    public void GetByCountryCode_WithInvalidCountryCode_ShouldReturnEmptyList()
    {
        // Act
        var cities = City.GetByCountryCode("XY");

        // Assert
        Assert.Empty(cities);
    }

    [Fact]
    public void GetByCountryCode_WithEmptyString_ShouldReturnEmptyList()
    {
        // Act
        var cities = City.GetByCountryCode("");

        // Assert
        Assert.Empty(cities);
    }

    [Theory]
    [InlineData("US", 3)]
    [InlineData("GB", 2)]
    [InlineData("JP", 2)]
    [InlineData("FR", 2)]
    public void GetByCountryCode_WithVariousCountries_ShouldReturnCorrectCount(string countryCode, int expectedCount)
    {
        // Act
        var cities = City.GetByCountryCode(countryCode);

        // Assert
        Assert.Equal(expectedCount, cities.Count);
    }
}
