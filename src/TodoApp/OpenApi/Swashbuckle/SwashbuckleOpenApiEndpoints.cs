// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Models;

namespace TodoApp.OpenApi.Swashbuckle;

public static class SwashbuckleOpenApiEndpoints
{
    public static IServiceCollection AddSwashbuckleOpenApi(this IServiceCollection services)
    {
        services.AddSwaggerGen((options) =>
        {
            var info = new OpenApiInfo()
            {
                // Add a title and version for the OpenAPI document
                Title = "Todo API (Swashbuckle.AspNetCore)",
                Version = "v1",
                // Add contact and license details for the API
                Contact = new()
                {
                    Name = "Martin Costello",
                    Url = new("https://github.com/martincostello/aspnetcore-openapi"),
                },
                License = new()
                {
                    Name = "Apache 2.0",
                    Url = new("https://www.apache.org/licenses/LICENSE-2.0"),
                }
            };

            options.SwaggerDoc(info.Version, info);

            // Configure bearer authentication using a JWT
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

            // Enable reading OpenAPI metadata from attributes
            options.EnableAnnotations();

            // Configure adding XML comments to operations and schemas
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TodoApp.xml"));

            // Add filters to add document-level tags to the OpenAPI document
            options.DocumentFilter<AddDocumentTagsFilter>();

            // Add server information to the OpenAPI document
            options.DocumentFilter<AddServersFilter>();

            // Add filters to add examples to the OpenAPI document
            options.OperationFilter<ExampleFilter>();
            options.SchemaFilter<ExampleFilter>();
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
