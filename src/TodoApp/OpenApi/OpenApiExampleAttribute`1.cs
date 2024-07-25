// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp.OpenApi;

/// <summary>
/// An attribute representing an example for an OpenAPI operation or schema.
/// </summary>
/// <typeparam name="T">The type of the schema.</typeparam>
public sealed class OpenApiExampleAttribute<T>() : OpenApiExampleAttribute<T, T>()
    where T : IExampleProvider<T>
{
}
