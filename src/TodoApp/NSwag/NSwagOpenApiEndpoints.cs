// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

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

                document.Tags.Add(new() { Name = "TodoApp" });

                // Remove the NSwag generator information
                document.Generator = null;
            };

            // Update errors to have a media type of "application/problem+json"
            options.OperationProcessors.Add(new UpdateProblemDetailsMediaTypeProcessor());

            // Remove NSwag-specific extension properties from parameters
            options.OperationProcessors.Add(new RemoveNSwagExtensions());
        });

        return services;
    }

    public static T UseNSwagOpenApi<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        builder.UseOpenApi(settings =>
        {
            settings.Path = "/nswag/{documentName}.json";

            // Show HTTP and HTTPS servers to match OpenAPI and Swashbuckle
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

    public class RemoveNSwagExtensions : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            foreach (var parameter in context.Parameters.Values)
            {
                parameter.Position = null;

                if (parameter.Kind is OpenApiParameterKind.Body)
                {
                    parameter.Name = null;
                }
            }

            return true;
        }
    }

    public class UpdateProblemDetailsMediaTypeProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            foreach ((string status, var response) in context.OperationDescription.Operation.Responses)
            {
                if (status.StartsWith('2'))
                {
                    continue;
                }

                foreach ((string key, var mediaType) in response.Content.ToDictionary())
                {
                    if (key is "application/json")
                    {
                        var responses = context.OperationDescription.Operation.Responses[status];
                        responses.Content["application/problem+json"] = mediaType;
                        responses.Content.Remove(key);
                    }
                }
            }

            return true;
        }
    }
}
