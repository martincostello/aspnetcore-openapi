// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TodoApp.OpenApi.AspNetCore;

public static class AspNetCoreOpenApiEndpoints
{
    public static IServiceCollection AddAspNetCoreOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            // Add a document transformer to customise the generated OpenAPI document
            options.AddDocumentTransformer((document, _, _) =>
            {
                // Add a title and version for the OpenAPI document
                document.Info.Title = "Todo API (ASP.NET Core OpenAPI)";
                document.Info.Description = "An API for managing Todo items.";
                document.Info.Version = "v1";

                // Add contact and license details for the API
                document.Info.Contact = new()
                {
                    Name = "Martin Costello",
                    Url = new("https://github.com/martincostello/aspnetcore-openapi"),
                };

                document.Info.License = new()
                {
                    Name = "Apache 2.0",
                    Url = new("https://www.apache.org/licenses/LICENSE-2.0"),
                };

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
                };

                document.Components ??= new();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                document.Components.SecuritySchemes[scheme.Reference.Id] = scheme;
                document.SecurityRequirements.Add(new() { [scheme] = [] });

                return Task.CompletedTask;
            });

            // Add a custom schema transformer to add descriptions from XML comments
            var descriptions = new AddSchemaDescriptionsTransformer();
            options.AddSchemaTransformer(descriptions);

            // Add transformer to add examples to OpenAPI parameters, requests, responses and schemas
            var examples = new AddExamplesTransformer();
            options.AddOperationTransformer(examples);
            options.AddSchemaTransformer(examples);
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
