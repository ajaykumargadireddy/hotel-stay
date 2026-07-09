using System;
using System.Collections.Generic;
using HotelStay.Domain.Enums;
using HotelStay.Infrastructure.Providers;

namespace HotelStay.Infrastructure.Providers.PremierStays;

public static class PremierStaysStubData
{
    public static readonly Dictionary<Guid, PremierStaysRoomEntry> Catalog = new()
    {
        // India - Mumbai (Domestic)
        {
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Provider = "PremierStays",
                Destination = "BOM",
                Location = "India",
                RoomType = RoomType.Standard,
                CheckIn = StubDates.IndiaCheckIn,
                CheckOut = StubDates.IndiaCheckOut,
                PerNightRate = 3000m,
                Currency = "INR",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "AC", "TV" },
                StarRating = 3
            }
        },
        {
            Guid.Parse("11111111-2222-2222-2222-222222222222"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("11111111-2222-2222-2222-222222222222"),
                Provider = "PremierStays",
                Destination = "BLR",
                Location = "India",
                RoomType = RoomType.Deluxe,
                CheckIn = StubDates.IndiaCheckIn,
                CheckOut = StubDates.IndiaCheckOut,
                PerNightRate = 5000m,
                Currency = "INR",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "AC", "TV", "Pool", "Gym" },
                StarRating = 4
            }
        },
        {
            Guid.Parse("11111111-3333-3333-3333-333333333333"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("11111111-3333-3333-3333-333333333333"),
                Provider = "PremierStays",
                Destination = "BOM",
                Location = "India",
                RoomType = RoomType.Suite,
                CheckIn = StubDates.IndiaCheckIn,
                CheckOut = StubDates.IndiaCheckOut,
                PerNightRate = 8000m,
                Currency = "INR",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "AC", "TV", "Pool", "Gym", "Spa", "Butler" },
                StarRating = 5
            }
        },

        // UK - London (International)
        {
            Guid.Parse("22222222-1111-1111-1111-111111111111"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("22222222-1111-1111-1111-111111111111"),
                Provider = "PremierStays",
                Destination = "LON",
                Location = "United Kingdom",
                RoomType = RoomType.Standard,
                CheckIn = StubDates.UkCheckIn,
                CheckOut = StubDates.UkCheckOut,
                PerNightRate = 100m,
                Currency = "GBP",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "Heating" },
                StarRating = 3
            }
        },
        {
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Provider = "PremierStays",
                Destination = "LON",
                Location = "United Kingdom",
                RoomType = RoomType.Deluxe,
                CheckIn = StubDates.UkCheckIn,
                CheckOut = StubDates.UkCheckOut,
                PerNightRate = 150m,
                Currency = "GBP",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "Heating", "Pool", "Gym" },
                StarRating = 4
            }
        },
        {
            Guid.Parse("22222222-3333-3333-3333-333333333333"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("22222222-3333-3333-3333-333333333333"),
                Provider = "PremierStays",
                Destination = "LON",
                Location = "United Kingdom",
                RoomType = RoomType.Suite,
                CheckIn = StubDates.UkCheckIn,
                CheckOut = StubDates.UkCheckOut,
                PerNightRate = 250m,
                Currency = "GBP",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "Heating", "Pool", "Gym", "Spa", "Concierge" },
                StarRating = 5
            }
        },

        // US - New York (International)
        {
            Guid.Parse("33333333-1111-1111-1111-111111111111"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("33333333-1111-1111-1111-111111111111"),
                Provider = "PremierStays",
                Destination = "NYC",
                Location = "United States",
                RoomType = RoomType.Standard,
                CheckIn = StubDates.UsCheckIn,
                CheckOut = StubDates.UsCheckOut,
                PerNightRate = 120m,
                Currency = "USD",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "AC" },
                StarRating = 3
            }
        },
        {
            Guid.Parse("33333333-2222-2222-2222-222222222222"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("33333333-2222-2222-2222-222222222222"),
                Provider = "PremierStays",
                Destination = "NYC",
                Location = "United States",
                RoomType = RoomType.Deluxe,
                CheckIn = StubDates.UsCheckIn,
                CheckOut = StubDates.UsCheckOut,
                PerNightRate = 180m,
                Currency = "USD",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "AC", "Gym", "BusinessCenter" },
                StarRating = 4
            }
        },
        {
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Provider = "PremierStays",
                Destination = "NYC",
                Location = "United States",
                RoomType = RoomType.Suite,
                CheckIn = StubDates.UsCheckIn,
                CheckOut = StubDates.UsCheckOut,
                PerNightRate = 300m,
                Currency = "USD",
                CancellationPolicy = "Free Cancellation Upto 48h",
                Amenities = new[] { "WiFi", "AC", "Gym", "BusinessCenter", "RooftopBar", "Spa" },
                StarRating = 5
            }
        },

        // NonRefundable entries - PremierStays
        {
            Guid.Parse("11111111-4444-4444-4444-444444444444"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("11111111-4444-4444-4444-444444444444"),
                Provider = "PremierStays",
                Destination = "BOM",
                Location = "India",
                RoomType = RoomType.Standard,
                CheckIn = StubDates.IndiaCheckIn,
                CheckOut = StubDates.IndiaCheckOut,
                PerNightRate = 2500m,
                Currency = "INR",
                CancellationPolicy = "NonRefundable",
                Amenities = new[] { "WiFi", "AC" },
                StarRating = 3
            }
        },
        {
            Guid.Parse("22222222-4444-4444-4444-444444444444"),
            new PremierStaysRoomEntry
            {
                RoomId = Guid.Parse("22222222-4444-4444-4444-444444444444"),
                Provider = "PremierStays",
                Destination = "LON",
                Location = "United Kingdom",
                RoomType = RoomType.Deluxe,
                CheckIn = StubDates.UkCheckIn,
                CheckOut = StubDates.UkCheckOut,
                PerNightRate = 130m,
                Currency = "GBP",
                CancellationPolicy = "NonRefundable",
                Amenities = new[] { "WiFi", "Heating" },
                StarRating = 4
            }
        }
    };
}
