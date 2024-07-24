// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoApp.Swashbuckle;

/// <summary>
/// A class representing an operation filter that adds the example to use for display in Swagger documentation.
/// </summary>
public class ExampleFilter : IOperationFilter, ISchemaFilter
{
    private static readonly TodoJsonSerializerContext Context = TodoJsonSerializerContext.Default;

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation != null && context?.ApiDescription != null && context.SchemaRepository != null)
        {
            AddResponseExamples(operation, context);
        }
    }

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != null)
        {
            var metadata = context.Type
                .GetCustomAttributes()
                .OfType<IOpenApiExampleMetadata>()
                .FirstOrDefault();

            if (metadata != null)
            {
                schema.Example = metadata.GenerateExample(Context);
            }
        }
    }

    private static T[] GetAttributes<T>(ApiDescription apiDescription)
    {
        IEnumerable<T> attributes = [];

        if (apiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
        {
            attributes = methodInfo.GetCustomAttributes().OfType<T>();
        }

        if (apiDescription.ActionDescriptor is not null)
        {
            attributes = [.. attributes, .. apiDescription.ActionDescriptor.EndpointMetadata.OfType<T>()];
        }

        return attributes.ToArray();
    }

    private static void AddResponseExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        var examples = GetAttributes<IOpenApiExampleMetadata>(context.ApiDescription);

        foreach (var metadata in examples)
        {
            if (!context.SchemaRepository.Schemas.TryGetValue(metadata.SchemaType.Name, out var schema))
            {
                continue;
            }

            var response = operation.Responses
                .SelectMany((p) => p.Value.Content)
                .Where((p) => p.Value.Schema.Reference.Id == metadata.SchemaType.Name)
                .Select((p) => p)
                .FirstOrDefault();

            if (!response.Equals(new KeyValuePair<string, OpenApiMediaType>()))
            {
                response.Value.Example = metadata.GenerateExample(Context);
            }
        }
    }
}
