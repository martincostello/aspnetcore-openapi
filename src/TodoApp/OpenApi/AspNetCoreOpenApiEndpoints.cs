// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Models;

namespace TodoApp;

public static class AspNetCoreOpenApiEndpoints
{
    public static IServiceCollection AddAspNetCoreOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = "Todo API (ASP.NET Core OpenAPI)";
                document.Info.Version = "v1";

                document.Info.Contact = new()
                {
                    Name = "Martin Costello",
                    Url = new("https://www.martincostello.com"),
                };

                document.Info.License = new()
                {
                    Name = "Apache 2.0",
                    Url = new("https://www.apache.org/licenses/LICENSE-2.0"),
                };

                var scheme = new OpenApiSecurityScheme()
                {
                    BearerFormat = "JSON Web Token",
                    Description = "Bearer authentication using a JWT.",
                    Scheme = "bearer",
                    Type = SecuritySchemeType.Http,
                    Reference = new()
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    },
                };

                document.Components ??= new();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                document.Components.SecuritySchemes[scheme.Reference.Id] = scheme;

                document.SecurityRequirements.Add(new() { [scheme] = [] });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static T UseAspnetCoreOpenApi<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        builder.MapOpenApi();

        return builder;
    }
}
