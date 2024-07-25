// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApp.OpenApi.Swashbuckle;

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

            options.DocumentFilter<AddDocumentTagsFilter>();
            options.DocumentFilter<AddServersFilter>();

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

    public class AddDocumentTagsFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags.Add(new() { Name = "TodoApp" });
        }
    }

    public class AddServersFilter(IHostEnvironment environment, IServer server) : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (environment.IsDevelopment() &&
                server.Features.Get<IServerAddressesFeature>()?.Addresses is { Count: > 0 } addresses)
            {
                swaggerDoc.Servers = addresses.Select(address => new OpenApiServer { Url = address }).ToList();
            }
        }
    }
}
