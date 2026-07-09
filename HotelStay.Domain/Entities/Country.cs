using System.Collections.Generic;

namespace HotelStay.Domain.Entities;

public class Country
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public static readonly List<Country> All = new()
    {
        new Country { Code = "IN", Name = "India" },
        new Country { Code = "GB", Name = "United Kingdom" },
        new Country { Code = "US", Name = "United States" },
        new Country { Code = "JP", Name = "Japan" },
        new Country { Code = "FR", Name = "France" }
    };
}
