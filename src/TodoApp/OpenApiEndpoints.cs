// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

public static class OpenApiEndpoints
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        services.AddAspNetCoreOpenApi();
        services.AddNSwagOpenApi();
        services.AddSwashbuckleOpenApi();

        return services;
    }

    public static T UseOpenApiEndpoints<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        builder.UseAspnetCoreOpenApi();
        builder.UseNSwagOpenApi();
        builder.UseSwashbuckleOpenApi();

        return builder;
    }
}
