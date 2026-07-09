using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Karambolo.Extensions.Logging.File;


namespace HotelStay.Api.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddLoggingProvider(
            this WebApplicationBuilder builder)
        {
            builder.Logging.AddFile(o =>
            {
                o.RootPath = builder.Environment.ContentRootPath;
                o.BasePath = "logs";

                o.Files = new[]
                {
                new LogFileOptions
                {
                    Path = "hotelstay-<date:yyyy-MM-dd>.txt",
                    MinLevel = new Dictionary<string, LogLevel>
                    {
                        ["Default"] = LogLevel.Error,
                        ["Microsoft"] = LogLevel.Error,
                        ["Microsoft.AspNetCore"] = LogLevel.Error
                    }
                }
            };

                o.DateFormat = "yyyy-MM-dd HH:mm:ss.fff";
                o.IncludeScopes = true;
            });

            return builder;
        }
    }
}
