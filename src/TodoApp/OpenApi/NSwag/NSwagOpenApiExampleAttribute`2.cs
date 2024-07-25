// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NSwag.Annotations;

namespace TodoApp.OpenApi.NSwag;

/// <summary>
/// A class representing an example for a type for a NSwag OpenAPI operation or shema.
/// </summary>
/// <typeparam name="TSchema">The type of the schema.</typeparam>
/// <typeparam name="TProvider">The type of the example provider.</typeparam>
public class NSwagOpenApiExampleAttribute<TSchema, TProvider>() :
    OpenApiOperationProcessorAttribute(typeof(OpenApiOperationExampleProcessor<TSchema, TProvider>))
    where TProvider : IExampleProvider<TSchema>;
