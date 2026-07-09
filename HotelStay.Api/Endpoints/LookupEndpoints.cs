using HotelStay.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace HotelStay.Api.Endpoints;

public static class LookupEndpoints
{
    public static void MapLookupEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/countries", () =>
        {
            return Results.Ok(Country.All);
        })
        .WithName("GetCountries")
        .WithTags("Lookups")
        .Produces<Country[]>(StatusCodes.Status200OK);

        app.MapGet("/cities", (string countryCode) =>
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                return Results.Ok(City.All);
            }

            var cities = City.GetByCountryCode(countryCode);
            return Results.Ok(cities);
        })
        .WithName("GetCities")
        .WithTags("Lookups")
        .Produces<City[]>(StatusCodes.Status200OK);
    }
}
