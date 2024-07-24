// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

public static class OpenApiEndpoints
{
    public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
    {
        // Configure Microsoft.AspNetCore.OpenApi documentation for the Todo API
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Todo API";
                document.Info.Version = "v1";
                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static T UseOpenApiEndpoints<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        // Add endpoint for Microsoft.AspNetCore.OpenApi
        builder.MapOpenApi();

        return builder;
    }
}
