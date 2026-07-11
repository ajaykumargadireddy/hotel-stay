using System;

namespace HotelStay.Infrastructure.Providers;

/// <summary>
/// Central source of future-relative check-in / check-out dates used by every
/// provider stub catalog. Values are captured once at type initialization from
/// <see cref="DateTime.Today"/>, so stub rooms are always in the future for the
/// lifetime of the process (Swagger, Postman, integration probes, etc.).
///
/// All regions start availability tomorrow (today + 1) with staggered end dates:
///   India   (BOM / BLR / DEL) : tomorrow  ..  today + 30   (30 days)
///   UK      (LON / MAN)       : tomorrow  ..  today + 45   (45 days)
///   US      (NYC / LAX / CHI) : tomorrow  ..  today + 60   (60 days)
///   Japan   (TYO / OSA)       : tomorrow  ..  today + 90   (90 days)
/// </summary>
public static class StubDates
{
    // All regions start tomorrow
    private static readonly DateTime StartDate = DateTime.Today.AddDays(1);

    // India (BOM, BLR, DEL) - Available for 30 days
    public static readonly DateTime IndiaCheckIn = StartDate;
    public static readonly DateTime IndiaCheckOut = DateTime.Today.AddDays(30);

    // United Kingdom (LON, MAN) - Available for 45 days
    public static readonly DateTime UkCheckIn = StartDate;
    public static readonly DateTime UkCheckOut = DateTime.Today.AddDays(45);

    // United States (NYC, LAX, CHI) - Available for 60 days
    public static readonly DateTime UsCheckIn = StartDate;
    public static readonly DateTime UsCheckOut = DateTime.Today.AddDays(60);

    // Japan (TYO, OSA) - Available for 90 days
    public static readonly DateTime JapanCheckIn = StartDate;
    public static readonly DateTime JapanCheckOut = DateTime.Today.AddDays(90);
}
