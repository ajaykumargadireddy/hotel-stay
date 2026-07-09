using HotelStay.Api.Middleware;
using Karambolo.Extensions.Logging.File;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;


namespace HotelStay.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}