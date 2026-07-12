using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using HotelStay.Application.Abstractions;
using HotelStay.Application.DTOs;
using HotelStay.Domain.Enums;
using HotelStay.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HotelStay.Api.Endpoints;

public static class HotelEndpoints
{
    public static IEndpointRouteBuilder MapHotelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/hotels");

        // GET /hotels/search
        group.MapGet("/search", async (
            [FromQuery] string destination,
            [FromQuery] DateTime? checkIn,
            [FromQuery] DateTime? checkOut,
            [FromQuery] RoomType? roomType,
            [FromServices] IHotelSearchService service,
            CancellationToken cancellationToken) =>
        {
            // Validate required parameters
            if (string.IsNullOrWhiteSpace(destination))
            {
                return Results.BadRequest(new { error = "Destination is required" });
            }

            if (!checkIn.HasValue)
            {
                return Results.BadRequest(new { error = "CheckIn is required" });
            }

            if (!checkOut.HasValue)
            {
                return Results.BadRequest(new { error = "CheckOut is required" });
            }

            // Validate checkOut > checkIn
            if (checkOut.Value <= checkIn.Value)
            {
                return Results.BadRequest(new { error = "CheckOut must be after CheckIn" });
            }

            var request = new HotelSearchRequest
            {
                Destination = destination,
                CheckIn = checkIn.Value,
                CheckOut = checkOut.Value,
                RoomType = roomType
            };

            var rooms = await service.SearchAsync(request, cancellationToken);
            return Results.Ok(new { results = rooms });
        })
        .WithName("SearchHotels")
        .Produces<IEnumerable<HotelSearchResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/reserve", async (
            [FromBody] ReservationRequest request,
            [FromServices] IReservationService service,
            CancellationToken cancellationToken) =>
                {
            var reservation = await service.ReserveAsync(request, cancellationToken);

            return Results.Text(
                reservation,
                statusCode: StatusCodes.Status201Created);
            })
            .WithName("ReserveHotel")
            .Produces<string>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        // GET /hotels/reservation/{reference}
        group.MapGet("/reservation/{reference}", async (
            [FromRoute] string reference,
            [FromServices] IReservationService service,
            CancellationToken cancellationToken) =>
        {
            var reservation = await service.GetByReferenceAsync(reference, cancellationToken);
            return Results.Ok(reservation);
        })
        .WithName("GetReservation")
        .Produces<ReservationResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
