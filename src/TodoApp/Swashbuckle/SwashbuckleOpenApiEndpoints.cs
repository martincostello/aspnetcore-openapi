// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Models;

namespace TodoApp;

public static class SwashbuckleOpenApiEndpoints
{
    public static IServiceCollection AddSwashbuckleOpenApi(this IServiceCollection services)
    {
        services.AddSwaggerGen((options) =>
        {
            options.SwaggerDoc("v1", new()
            {
                Contact = new()
                {
                    Name = "Martin Costello",
                    Url = new("https://www.martincostello.com"),
                },
                License = new()
                {
                    Name = "Apache 2.0",
                    Url = new("https://www.apache.org/licenses/LICENSE-2.0"),
                },
                Title = "Todo API (Swashbuckle.AspNetCore)",
                Version = "v1"
            });

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
                UnresolvedReference = false,
            };
            options.AddSecurityDefinition(scheme.Reference.Id, scheme);
            options.AddSecurityRequirement(new() { [scheme] = [] });

            options.EnableAnnotations();
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TodoApp.xml"));
        });

        return services;
    }

    public static T UseSwashbuckleOpenApi<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        builder.UseSwagger();

        return builder;
    }
}
