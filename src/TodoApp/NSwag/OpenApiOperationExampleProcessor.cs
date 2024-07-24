// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace TodoApp.NSwag;

/// <summary>
/// A class representing an processor for OpenAPI operation examples.
/// </summary>
/// <typeparam name="TSchema">The type of the schema.</typeparam>
/// <typeparam name="TProvider">The type of the example provider.</typeparam>
public sealed class OpenApiOperationExampleProcessor<TSchema, TProvider> : IOperationProcessor
    where TProvider : IExampleProvider<TSchema>
{
    /// <inheritdoc/>
    public bool Process(OperationProcessorContext context)
    {
        var examples = context is AspNetCoreOperationProcessorContext aspnet
            ? aspnet.ApiDescription.ActionDescriptor.EndpointMetadata
                .OfType<IOpenApiExampleMetadata>()
                .ToArray()
            : context.MethodInfo.GetCustomAttributes()
                .OfType<IOpenApiExampleMetadata>()
                .ToArray();

        foreach ((var info, var parameter) in context.Parameters)
        {
            if (parameter.Example is not null)
            {
                continue;
            }

            var metadata = info.GetCustomAttributes()
                .OfType<IOpenApiExampleMetadata>()
                .FirstOrDefault();

            metadata ??= info.ParameterType.GetCustomAttributes()
                .OfType<IOpenApiExampleMetadata>()
                .FirstOrDefault();

            metadata ??= examples.FirstOrDefault((p) => p.SchemaType == info.ParameterType);

            if (metadata is { } example)
            {
                parameter.Example = CreateExample(metadata.GenerateExample());
            }
        }

        if (context.Document.Components.Schemas.TryGetValue(typeof(TSchema).Name, out var schema))
        {
            object? example = TProvider.GenerateExample();

            if (example is ProblemDetails problem)
            {
                schema.AdditionalPropertiesSchema = NJsonSchema.JsonSchema.CreateAnySchema();
            }

            schema.Example = CreateExample(example);

            foreach (var parameter in context.OperationDescription.Operation.Parameters.Where((p) => p.Schema?.Reference == schema))
            {
                parameter.Example = schema.Example;
            }

            foreach (var response in context.OperationDescription.Operation.Responses.Values)
            {
                foreach (var mediaType in response.Content.Values.Where((p) => p.Schema?.Reference == schema))
                {
                    mediaType.Example = schema.Example;
                }
            }
        }

        return true;
    }

    private static JToken? CreateExample(object? example)
    {
        // Round-trip the value through the serializer to a JToken so that NSwag uses the right property names
        var json = JsonSerializer.Serialize(example, example?.GetType() ?? typeof(TSchema), TodoJsonSerializerContext.Default);

        var serialized = JsonNode.Parse(json);

        if (serialized is null)
        {
            return null;
        }

        json = serialized!.ToJsonString();
        return JToken.Parse(json);
    }
}
