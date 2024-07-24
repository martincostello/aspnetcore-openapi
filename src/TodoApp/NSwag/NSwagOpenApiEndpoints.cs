// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NSwag;

namespace TodoApp;

public static class NSwagOpenApiEndpoints
{
    public static IServiceCollection AddNSwagOpenApi(this IServiceCollection services)
    {
        services.AddOpenApiDocument(options =>
        {
            options.Title = "Todo API (NSwag)";
            options.Version = "v1";

            options.PostProcess = document =>
            {
                document.Generator = null;

                document.Info.Contact = new()
                {
                    Name = "Martin Costello",
                    Url = "https://www.martincostello.com",
                };

                document.Info.License = new()
                {
                    Name = "Apache 2.0",
                    Url = "https://www.apache.org/licenses/LICENSE-2.0",
                };

                document.SecurityDefinitions.Add("Bearer", new()
                {
                    BearerFormat = "JSON Web Token",
                    Description = "Bearer authentication using a JWT.",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Scheme = "bearer",
                    Type = OpenApiSecuritySchemeType.Http,
                });
                document.Security.Add(new() { ["Bearer"] = [] });
            };
        });

        return services;
    }

    public static T UseNSwagOpenApi<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        builder.UseOpenApi(settings => settings.Path = "/nswag/{documentName}.json");

        return builder;
    }
}
