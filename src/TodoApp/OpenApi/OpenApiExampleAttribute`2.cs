// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace TodoApp.OpenApi;

/// <summary>
/// An attribute representing an example for an OpenAPI operation or schema.
/// </summary>
/// <typeparam name="TSchema">The type of the schema.</typeparam>
/// <typeparam name="TProvider">The type of the example provider.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
public class OpenApiExampleAttribute<TSchema, TProvider> : Attribute, IOpenApiExampleMetadata
    where TProvider : IExampleProvider<TSchema>
{
    /// <inheritdoc/>
    public Type SchemaType => typeof(TSchema);

    /// <summary>
    /// Generates the example to use.
    /// </summary>
    /// <returns>
    /// A <typeparamref name="TSchema"/> that should be used as the example.
    /// </returns>
    public virtual TSchema GenerateExample() => TProvider.GenerateExample();

    /// <inheritdoc/>
    object? IOpenApiExampleMetadata.GenerateExample() => GenerateExample();

    /// <inheritdoc/>
    JsonNode? IOpenApiExampleMetadata.GenerateExample(JsonSerializerContext context)
        => ExampleFormatter.AsJson(GenerateExample(), context);
}
