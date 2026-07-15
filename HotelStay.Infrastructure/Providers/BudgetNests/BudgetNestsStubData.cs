using HotelStay.Domain.Enums;
using HotelStay.Infrastructure.Providers;
using System;
using System.Collections.Generic;

namespace HotelStay.Infrastructure.Providers.BudgetNests;

public static class BudgetNestsStubData
{
    public static readonly Dictionary<Guid, BudgetNestsRoomEntry> Catalog = new()
    {
        // India - Mumbai (Domestic)
        {
            Guid.Parse("44444444-1111-1111-1111-111111111111"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("44444444-1111-1111-1111-111111111111"),
                provider = "BudgetNests",
                destination = "BOM",
                location = "India",
                room_type = RoomType.Standard,
                check_in = StubDates.IndiaCheckIn,
                check_out = StubDates.IndiaCheckOut,
                per_night_rate = 2000m,
                currency = "INR",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),
                star_rating = null,
                available = true
            }
        },
        {
            Guid.Parse("44444444-2222-2222-2222-222222222222"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("44444444-2222-2222-2222-222222222222"),
                provider = "BudgetNests",
                destination = "DEL",
                location = "India",
                room_type = RoomType.Deluxe,
                check_in = StubDates.IndiaCheckIn,
                check_out = StubDates.IndiaCheckOut,
                per_night_rate = 3500m,
                currency = "INR",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),  // BudgetNests: minimal data - rate and policy only
                available = true,
                star_rating = null
            }
        },
        {
            Guid.Parse("44444444-3333-3333-3333-333333333333"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("44444444-3333-3333-3333-333333333333"),
                provider = "BudgetNests",
                destination = "BOM",
                location = "India",
                room_type = RoomType.Suite,
                check_in = StubDates.IndiaCheckIn,
                check_out = StubDates.IndiaCheckOut,
                per_night_rate = 6000m,
                currency = "INR",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),  // BudgetNests: minimal data - rate and policy only
                available = true,
                star_rating = null
            }
        },

        // UK - London (International)
        {
            Guid.Parse("55555555-1111-1111-1111-111111111111"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("55555555-1111-1111-1111-111111111111"),
                provider = "BudgetNests",
                destination = "LON",
                location = "United Kingdom",
                room_type = RoomType.Standard,
                check_in = StubDates.UkCheckIn,
                check_out = StubDates.UkCheckOut,
                per_night_rate = 80m,
                currency = "GBP",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),
                available = true,
                star_rating = null
            }
        },
        {
            Guid.Parse("55555555-2222-2222-2222-222222222222"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("55555555-2222-2222-2222-222222222222"),
                provider = "BudgetNests",
                destination = "MAN",
                location = "United Kingdom",
                room_type = RoomType.Deluxe,
                check_in = StubDates.UkCheckIn,
                check_out = StubDates.UkCheckOut,
                per_night_rate = 120m,
                currency = "GBP",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),  // BudgetNests: minimal data - rate and policy onlyg>(),  // BudgetNests: minimal data - rate and policy only
                available = true,
                star_rating = null
            }
        },
        {
            Guid.Parse("55555555-3333-3333-3333-333333333333"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("55555555-3333-3333-3333-333333333333"),
                provider = "BudgetNests",
                destination = "LON",
                location = "United Kingdom",
                room_type = RoomType.Suite,
                check_in = StubDates.UkCheckIn,
                check_out = StubDates.UkCheckOut,
                per_night_rate = 200m,
                currency = "GBP",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),  // BudgetNests: minimal data - rate and policy only
                available = true,
                star_rating = null
            }
        },

        // US - New York (International)
        {
            Guid.Parse("66666666-1111-1111-1111-111111111111"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("66666666-1111-1111-1111-111111111111"),
                provider = "BudgetNests",
                destination = "NYC",
                location = "United States",
                room_type = RoomType.Standard,
                check_in = StubDates.UsCheckIn,
                check_out = StubDates.UsCheckOut,
                per_night_rate = 90m,
                currency = "USD",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),
               available = true,
                star_rating = null
            }
        },
        {
            Guid.Parse("66666666-2222-2222-2222-222222222222"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("66666666-2222-2222-2222-222222222222"),
                provider = "BudgetNests",
                destination = "NYC",
                location = "United States",
                room_type = RoomType.Deluxe,
                check_in = StubDates.UsCheckIn,
                check_out = StubDates.UsCheckOut,
                per_night_rate = 140m,
                currency = "USD",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),  // BudgetNests: minimal data - rate and policy only
                available = true,
                star_rating = null
            }
        },
        {
            Guid.Parse("66666666-3333-3333-3333-333333333333"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("66666666-3333-3333-3333-333333333333"),
                provider = "BudgetNests",
                destination = "NYC",
                location = "United States",
                room_type = RoomType.Suite,
                check_in = StubDates.UsCheckIn,
                check_out = StubDates.UsCheckOut,
                per_night_rate = 220m,
                currency = "USD",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),  // BudgetNests: minimal data - rate and policy only
               available = true,
                star_rating = null
            }
        },

        // NonRefundable entries - BudgetNests
        {
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                provider = "BudgetNests",
                destination = "BOM",
                location = "India",
                room_type = RoomType.Standard,
                check_in = StubDates.IndiaCheckIn,
                check_out = StubDates.IndiaCheckOut,
                per_night_rate = 1800m,
                currency = "INR",
                cancellation_policy = "NonRefundable",
                amenities = Array.Empty<string>(),
                available = true,
                star_rating = null
            }
        },
        {
            Guid.Parse("55555555-4444-4444-4444-444444444444"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("55555555-4444-4444-4444-444444444444"),
                provider = "BudgetNests",
                destination = "TYO",
                location = "Japan",
                room_type = RoomType.Suite,
                check_in = StubDates.JapanCheckIn,
                check_out = StubDates.JapanCheckOut,
                per_night_rate = 180m,
                currency = "JPY",
                cancellation_policy = "NonRefundable",
                amenities = Array.Empty<string>(),
                available = true,
                star_rating = null
            }
        },

        // Unavailable entry (to test filtering)
        {
            Guid.Parse("66666666-4444-4444-4444-444444444444"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("66666666-4444-4444-4444-444444444444"),
                provider = "BudgetNests",
                destination = "NYC",
                location = "United States",
                room_type = RoomType.Standard,
                check_in = StubDates.UsCheckIn,
                check_out = StubDates.UsCheckOut,
                per_night_rate = 85m,
                currency = "USD",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),
                available = false,  // This room should be filtered out
                star_rating = null
            }
        },
        {
            Guid.Parse("44444444-5555-5555-5555-555555555555"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("44444444-5555-5555-5555-555555555555"),
                provider = "BudgetNests",
                destination = "DEL",
                location = "India",
                room_type = RoomType.Deluxe,
                check_in = StubDates.IndiaCheckIn,
                check_out = StubDates.IndiaCheckOut,
                per_night_rate = 3200m,
                currency = "INR",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),
                available = false,
                star_rating = null
            }
        },
        {
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                provider = "BudgetNests",
                destination = "LON",
                location = "United Kingdom",
                room_type = RoomType.Standard,
                check_in = StubDates.UkCheckIn,
                check_out = StubDates.UkCheckOut,
                per_night_rate = 78m,
                currency = "GBP",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),
                available = false,
                star_rating = null
            }
        },
        {
            Guid.Parse("66666666-5555-5555-5555-555555555555"),
            new BudgetNestsRoomEntry
            {
                room_id = Guid.Parse("66666666-5555-5555-5555-555555555555"),
                provider = "BudgetNests",
                destination = "NYC",
                location = "United States",
                room_type = RoomType.Suite,
                check_in = StubDates.UsCheckIn,
                check_out = StubDates.UsCheckOut,
                per_night_rate = 210m,
                currency = "USD",
                cancellation_policy = "Flexible",
                amenities = Array.Empty<string>(),
                available = false,
                star_rating = null
            }
        }
    };
}
