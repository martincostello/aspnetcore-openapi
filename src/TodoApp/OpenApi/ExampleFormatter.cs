// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace TodoApp.OpenApi;

/// <summary>
/// A class containing methods to help format JSON examples for OpenAPI.
/// </summary>
internal static class ExampleFormatter
{
    /// <summary>
    /// Formats the example for the specified type.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema.</typeparam>
    /// <typeparam name="TProvider">The type of the example provider.</typeparam>
    /// <param name="context">The JSON serializer context to use.</param>
    /// <returns>
    /// The <see cref="JsonNode"/> to use as the example.
    /// </returns>
    public static JsonNode? AsJson<TSchema, TProvider>(JsonSerializerContext context)
        where TProvider : IExampleProvider<TSchema>
        => AsJson(TProvider.GenerateExample(), context);

    /// <summary>
    /// Formats the specified value as JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="example">The example value to format as JSON.</param>
    /// <param name="context">The JSON serializer context to use.</param>
    /// <returns>
    /// The <see cref="JsonNode"/> to use as the example.
    /// </returns>
    public static JsonNode? AsJson<T>(T example, JsonSerializerContext context)
    {
        // Apply any formatting rules configured for the API (e.g. camel casing)
        var json = JsonSerializer.Serialize(example, typeof(T), context);
        return JsonNode.Parse(json);
    }
}
