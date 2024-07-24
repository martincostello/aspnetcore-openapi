// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

public static class SwashbuckleOpenApiEndpoints
{
    public static IServiceCollection AddSwashbuckleOpenApi(this IServiceCollection services)
    {
        // TODO
        return services;
    }

    public static T UseSwashbuckleOpenApi<T>(this T builder)
        where T : IApplicationBuilder, IEndpointRouteBuilder
    {
        // TODO
        return builder;
    }
}
