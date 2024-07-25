// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using NSwag;

namespace TodoApp.OpenApi.NSwag;

public static class NSwagOpenApiEndpoints
{
    public static IServiceCollection AddNSwagOpenApi(this IServiceCollection services)
    {
        services.AddOpenApiDocument(options =>
        {
            // Add a title and version for the OpenAPI document
            options.Title = "Todo API (NSwag)";
            options.Version = "v1";

            // Configure a delegate to customize the generated OpenAPI document
            options.PostProcess = document =>
            {
                // Add contact and license details for the API
                document.Info.Contact = new()
                {
                    Name = "Martin Costello",
                    Url = "https://github.com/martincostello/aspnetcore-openapi",
                };

                document.Info.License = new()
                {
                    Name = "Apache 2.0",
                    Url = "https://www.apache.org/licenses/LICENSE-2.0",
                };

                // Configure bearer authentication using a JWT
                document.SecurityDefinitions.Add("Bearer", new()
                {
                    BearerFormat = "JSON Web Token",
                    Description = "Bearer authentication using a JWT.",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Scheme = "bearer",
                    Type = OpenApiSecuritySchemeType.Http,
                });
                document.Security.Add(new() { ["Bearer"] = [] });

                // Add document-level tags
                document.Tags.Add(new() { Name = "TodoApp" });

                // Remove the NSwag generator information
                document.Generator = null;
            };

            // Update errors to have a media type of "application/problem+json"
            options.OperationProcessors.Add(new UpdateProblemDetailsMediaTypeProcessor());

            // Remove NSwag-specific extension properties from the operations
            options.OperationProcessors.Add(new RemoveNSwagExtensionsProcessor());
        });

        return services;
    }

    public static T UseNSwagOpenApi<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        builder.UseOpenApi(settings =>
        {
            // Configure the path for the OpenAPI document to not collide with the default for ASP.NET Core
            settings.Path = "/nswag/{documentName}.json";

            // Show HTTP and HTTPS servers when developing locally to match ASP.NET Core
            var environment = builder.ApplicationServices.GetRequiredService<IHostEnvironment>();
            if (environment.IsDevelopment())
            {
                settings.PostProcess = (document, request) =>
                {
                    var server = builder.ApplicationServices.GetRequiredService<IServer>();

                    if (server.Features.Get<IServerAddressesFeature>()?.Addresses is { Count: > 0 } addresses)
                    {
                        document.Schemes = [OpenApiSchema.Https, OpenApiSchema.Http];
                        document.Servers.Clear();

                        foreach (var address in addresses)
                        {
                            document.Servers.Add(new() { Url = address });
                        }
                    }
                };
            };
        });

        return builder;
    }
}
