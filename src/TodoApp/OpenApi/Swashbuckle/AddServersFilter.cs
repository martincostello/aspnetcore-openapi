// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApp.OpenApi.Swashbuckle;

/// <summary>
/// A Swashbuckle document filter that adds the servers to the document.
/// </summary>
/// <param name="environment">The current environment.</param>
/// <param name="server">The current server.</param>
public class AddServersFilter(IHostEnvironment environment, IServer server) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Show HTTP and HTTPS servers when developing locally to match ASP.NET Core
        if (environment.IsDevelopment() &&
            server.Features.Get<IServerAddressesFeature>()?.Addresses is { Count: > 0 } addresses)
        {
            swaggerDoc.Servers = addresses.Select(address => new OpenApiServer { Url = address }).ToList();
        }
    }
}
