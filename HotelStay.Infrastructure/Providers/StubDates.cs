using System;

namespace HotelStay.Infrastructure.Providers;

/// <summary>
/// Central source of future-relative check-in / check-out dates used by every
/// provider stub catalog. Values are captured once at type initialization from
/// <see cref="DateTime.Today"/>, so stub rooms are always in the future for the
/// lifetime of the process (Swagger, Postman, integration probes, etc.).
///
/// Region date bands (all 5-night stays):
///   India   (BOM / BLR / DEL) : today + 30  ..  today + 35
///   UK      (LON / MAN)       : today + 45  ..  today + 50
///   US      (NYC)             : today + 60  ..  today + 65
///   Japan   (TYO)             : today + 75  ..  today + 80
/// </summary>
public static class StubDates
{
    // India (BOM, BLR, DEL)
    public static readonly DateTime IndiaCheckIn = DateTime.Today.AddDays(30);
    public static readonly DateTime IndiaCheckOut = DateTime.Today.AddDays(35);

    // United Kingdom (LON, MAN)
    public static readonly DateTime UkCheckIn = DateTime.Today.AddDays(45);
    public static readonly DateTime UkCheckOut = DateTime.Today.AddDays(50);

    // United States (NYC)
    public static readonly DateTime UsCheckIn = DateTime.Today.AddDays(60);
    public static readonly DateTime UsCheckOut = DateTime.Today.AddDays(65);

    // Japan (TYO)
    public static readonly DateTime JapanCheckIn = DateTime.Today.AddDays(75);
    public static readonly DateTime JapanCheckOut = DateTime.Today.AddDays(80);
}
