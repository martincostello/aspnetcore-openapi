// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp.OpenApi.NSwag;

/// <summary>
/// A class representing an example for a type for a NSwag OpenAPI operation or shema.
/// </summary>
/// <typeparam name="T">The type of the schema.</typeparam>
public class NSwagOpenApiExampleAttribute<T> : NSwagOpenApiExampleAttribute<T, T>
    where T : IExampleProvider<T>;
