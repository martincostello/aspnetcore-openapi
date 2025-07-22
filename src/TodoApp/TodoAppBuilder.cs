// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.HttpOverrides;

namespace TodoApp;

public static class TodoAppBuilder
{
    public static WebApplicationBuilder AddTodoApp(this WebApplicationBuilder builder)
    {
        // Configure the Todo repository and associated services
        builder.Services.AddTodoApi();

        // Add Razor Pages to render the UI
        builder.Services.AddRazorPages();

        // Configure OpenAPI documentation for the Todo API
        builder.Services.AddOpenApiServices();

        if (string.Equals(builder.Configuration["CODESPACES"], "true", StringComparison.OrdinalIgnoreCase))
        {
            // When running in GitHub Codespaces, X-Forwarded-Host also needs to be set
            builder.Services.Configure<ForwardedHeadersOptions>(
                options => options.ForwardedHeaders |= ForwardedHeaders.XForwardedHost);
        }

        return builder;
    }

    public static WebApplication UseTodoApp(this WebApplication app)
    {
        // Configure error handling
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
        }

        app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

        // Require use of HTTPS in production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        // Add static files for JavaScript, CSS and OpenAPI
        app.UseStaticFiles();

        // Add endpoints for OpenAPI
        app.UseOpenApiEndpoints();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Test");
        });

        // Add the HTTP endpoints
        app.MapTodoApiRoutes();

        // Add Razor Pages for the UI
        app.MapRazorPages();

        return app;
    }
}
