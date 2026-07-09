using System.Collections.Generic;
using System.Linq;

namespace HotelStay.Domain.Entities;

public class City
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;

    public static readonly List<City> All = new()
    {
        // India
        new City { Code = "BOM", Name = "Mumbai", CountryCode = "IN" },
        new City { Code = "DEL", Name = "Delhi", CountryCode = "IN" },
        new City { Code = "BLR", Name = "Bangalore", CountryCode = "IN" },
        
        // United Kingdom
        new City { Code = "LON", Name = "London", CountryCode = "GB" },
        new City { Code = "MAN", Name = "Manchester", CountryCode = "GB" },
        
        // United States
        new City { Code = "NYC", Name = "New York", CountryCode = "US" },
        new City { Code = "LAX", Name = "Los Angeles", CountryCode = "US" },
        new City { Code = "CHI", Name = "Chicago", CountryCode = "US" },
        
        // Japan
        new City { Code = "TYO", Name = "Tokyo", CountryCode = "JP" },
        new City { Code = "OSA", Name = "Osaka", CountryCode = "JP" },
        
        // France
        new City { Code = "PAR", Name = "Paris", CountryCode = "FR" },
        new City { Code = "NCE", Name = "Nice", CountryCode = "FR" }
    };

    public static City? GetByCode(string code)
    {
        return All.FirstOrDefault(c => c.Code.Equals(code, System.StringComparison.OrdinalIgnoreCase));
    }

    public static List<City> GetByCountryCode(string countryCode)
    {
        return All.Where(c => c.CountryCode.Equals(countryCode, System.StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
