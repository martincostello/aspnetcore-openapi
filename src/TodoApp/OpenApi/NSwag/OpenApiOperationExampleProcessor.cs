// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace TodoApp.OpenApi.NSwag;

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
        // Get all the examples that may apply to the operation through attributes
        // configured globally, on an API group, or on a specific endpoint.
        var examples = context is AspNetCoreOperationProcessorContext aspnet
            ? aspnet.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<IOpenApiExampleMetadata>().ToArray()
            : context.MethodInfo.GetExampleMetadata().ToArray();

        // Add examples for any parameters of the operation
        foreach ((var info, var parameter) in context.Parameters)
        {
            if (parameter.Example is not null)
            {
                continue;
            }

            // Find the example for the argument either as a parameter attribute,
            // an attribute on the parameter's type, or metadata from the endpoint.
            var metadata =
                info.GetExampleMetadata().FirstOrDefault() ??
                info.ParameterType.GetExampleMetadata().FirstOrDefault() ??
                examples.FirstOrDefault((p) => p.SchemaType == info.ParameterType);

            if (metadata is not null)
            {
                parameter.Example = CreateExample(metadata.GenerateExample());
            }
        }

        // Add examples for any schemas associated with the operation
        if (context.Document.Components.Schemas.TryGetValue(typeof(TSchema).Name, out var schema))
        {
            var example = TProvider.GenerateExample();

            // We cannot change ProblemDetails directly, so we need to adjust it if we see it
            if (example is ProblemDetails problem)
            {
                schema.AdditionalPropertiesSchema = NJsonSchema.JsonSchema.CreateAnySchema();
            }

            schema.Example = CreateExample(example);

            foreach (var parameter in context.OperationDescription.Operation.Parameters.Where((p) => p.Schema?.Reference == schema))
            {
                parameter.Example ??= schema.Example;
            }

            foreach (var response in context.OperationDescription.Operation.Responses.Values)
            {
                foreach (var mediaType in response.Content.Values.Where((p) => p.Schema?.Reference == schema))
                {
                    mediaType.Example ??= schema.Example;
                }
            }
        }

        return true;
    }

    private static JToken? CreateExample(object? example)
    {
        // Round-trip the value through the System.Text.Json serializer to a
        // JToken so that NSwag uses the right property names. Otherwise serialization
        // attributes for Newtonsoft.Json need to be added to the models to get
        // equivalent behavior when the property/field names are serialized.
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
